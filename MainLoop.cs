using UnityEngine;
using System.Collections.Generic;
using System;

public class MainLoop : MonoBehaviour {
    public GameObject MainCamera;

    public CharacterCreator Character;

    Transform mPMRootTranform;
    List<IKTargetUI> mIKTargetUIList;
    SkeletonGhoster mSkeletonGhoster;

    bool mPaused = true;

    void Pause() {
        Character.SetSkeletonAnglesFromPM();
        Character.ShowUI();
        GhostCreator.CreateGhosts();
        mPaused = true;
    }

    void UnPause() {
        Character.SetPMTargetAnglesFromSkeleton();
        ChainLineRenderer.HideAll();
        Character.HideUI();
        GhostCreator.DeleteGhosts();
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
        GhostCreator.RestoreGhostState();
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

        Pause();
    }
}
