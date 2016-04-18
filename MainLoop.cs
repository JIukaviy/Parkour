using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using System;

public class MainLoop : MonoBehaviour {
    public GameObject MainCamera;

    public CharacterCreator Character;

    public ReplayRecorder MainReplayRecorder;
    public ReplayPlayer MainReplayPlayer;

    public ReplayRecorder GhostReplayRecorder;
    public ReplayPlayer GhostReplayPlayer;

    public OnCollideInformer FinishInformer;

    public float GhostLivingTime;

    Transform mPMRootTranform;
    SkeletonGhoster mSkeletonGhoster;

    bool mPaused = true;

    void Pause() {
        MainReplayRecorder.PauseRecording();
        Character.ShowUI();
        Character.SetSkeletonAnglesFromPM();
        GhostCreator.CreateGhosts(GhostLivingTime);
        mPaused = true;
    }

    void UnPause() {
        ChainLineRenderer.HideAll();
        Character.HideUI();
        GhostCreator.DeleteGhosts();
        MainReplayRecorder.ContinueRecording();
        mPaused = false;
    }

    public void Clicked() {
        if (mPaused) {
            UnPause();
        } else {
            Pause();
        }
    }

    void MoveGhostsReplayToMain() {
        Replay ghostsReplay;
        if (GhostReplayPlayer.playing) {
            ghostsReplay = GhostReplayPlayer.replay;
            GhostReplayPlayer.Stop();
        } else {
            ghostsReplay = GhostReplayRecorder.StopRecording();
        }
        ghostsReplay.ReplaceGameObjects(GhostCreator.ghostToOriginal);
        MainReplayRecorder.ConcatenateReplay(ghostsReplay);
    }

    void OnIKTargetUIDown(object sender, EventArgs args) {
        //GhostCreator.DeleteGhosts();
    }

    void OnIKTargetUIUp(object sender, EventArgs args) {
        Character.SetPMTargetAnglesFromSkeleton();
        GhostCreator.RestoreGhostState(GhostLivingTime);
    }

    void OnFinish(object sender, EventArgs args) {
        Replay replay = MainReplayRecorder.StopRecording();
        Character.HideUI();
        GhostCreator.DeleteGhosts();
        Destroy(GameObject.Find("PlayButton"));
        MainReplayPlayer.Play(replay);
    }

    void OnIKTargetUIPosChanged(object sender, EventArgs args) {
        GhostReplayPlayer.Stop();
        GhostReplayRecorder.EraseRecorded();
        GhostReplayRecorder.ContinueRecording();
    }

    void OnGhostReplayEnd(object sender, EventArgs args) {
        GhostReplayPlayer.ToStart();
        GhostReplayPlayer.Continue();
        Debug.Log("GHOSTS REPLAY IS ENDED");
    }

    void OnMainReplayEnd(object sender, EventArgs args) {
        MainReplayPlayer.ToStart();
        MainReplayPlayer.Continue();
    }

    void OnFinishMainReplayEnd(object sender, EventArgs args) {
        MainReplayPlayer.ToStart();
        MainReplayPlayer.Continue();
    }

    void OnGhostLivingTimeExceeded(object sender, EventArgs args) {
        GhostCreator.RestoreGhostState(GhostLivingTime);
    }

    void Start() {
        Physics2D.gravity = new Vector2(0, -3);

        TargetFollower targetFollower = MainCamera.AddComponent<TargetFollower>();
        targetFollower.Target = Character.physicsModel.GetObjectByName("Pelvis").GetComponent<Transform>();
        targetFollower.zDistance = -10;
        targetFollower.Speed = 0.01f;

        mSkeletonGhoster = new SkeletonGhoster(Character.physicsModel, Character.skeleton);

        Character.spineIKTargetUI.OnDown = OnIKTargetUIDown;
        Character.leftHandIKTargetUI.OnDown = OnIKTargetUIDown;
        Character.rightHandIKTargetUI.OnDown = OnIKTargetUIDown;
        Character.leftLegIKTargetUI.OnDown = OnIKTargetUIDown;
        Character.rightLegIKTargetUI.OnDown = OnIKTargetUIDown;

        Character.spineIKTargetUI.OnUp = OnIKTargetUIUp;
        Character.leftHandIKTargetUI.OnUp = OnIKTargetUIUp;
        Character.rightHandIKTargetUI.OnUp = OnIKTargetUIUp;
        Character.leftLegIKTargetUI.OnUp = OnIKTargetUIUp;
        Character.rightLegIKTargetUI.OnUp = OnIKTargetUIUp;

        //Character.OnIKTargetUIPosChanged += OnIKTargetUIPosChanged;

        GhostCreator.RegisterLayerConverter(LayerMask.NameToLayer("Character"), LayerMask.NameToLayer("GhostCharacter"));
        GhostCreator.RegisterLayerConverter(LayerMask.NameToLayer("Dynamic"), LayerMask.NameToLayer("Ghost"));
        GhostCreator.OnGhostLivingTimeExceeded += OnGhostLivingTimeExceeded;

        MainReplayRecorder.timeDelta = 0.1f;
        MainReplayRecorder.StartRecording("Dynamic");
        MainReplayPlayer.OnReplayEnd += OnMainReplayEnd;
        FinishInformer.OnFinish += OnFinish;

        Pause();
    }

}
