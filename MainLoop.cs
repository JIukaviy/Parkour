using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using System;

public class MainLoop : MonoBehaviour {
    public GameObject MainCamera;

    public CharacterCreator Character;

    public ReplayRecorder MainReplayRecorder;
    public ReplayPlayer MainReplayPlayer;

    public OnCollideInformer FinishInformer;

    public float GhostLivingTime;

    public ButtonListener PlayButton;

    public ButtonListener LGrabButton;
    public ButtonListener RGrabButton;

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
        HideControllerUI();
        MainReplayPlayer.Play(replay);
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

    void OnPlayButtonPress(object sender, EventArgs args) {
        UnPause();
    }

    void OnPlayButtonRelease(object sender, EventArgs args) {
        Pause();
    }

    void OnLGrabButtonPress(object sender, EventArgs args) {
        Character.leftHand.ToggleGrab();
        GhostCreator.RestoreGhostState(GhostLivingTime);
    }

    void OnRGrabButtonPress(object sender, EventArgs args) {
        Character.rightHand.ToggleGrab();
        GhostCreator.RestoreGhostState(GhostLivingTime);
    }

    void HideControllerUI() {
        GameObject[] UIs = GameObject.FindGameObjectsWithTag("ControllerUI");

        foreach (GameObject ui in UIs) {
            ui.SetActive(false);
        }
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

        PlayButton.OnPress += OnPlayButtonPress;
        PlayButton.OnRelease += OnPlayButtonRelease;

        LGrabButton.OnPress += OnLGrabButtonPress;
        RGrabButton.OnPress += OnRGrabButtonPress;

        Pause();
    }
}
