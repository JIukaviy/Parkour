using UnityEngine;
using System.Collections.Generic;

public class MainLoop : MonoBehaviour {
    public GameObject MainCamera;

    public CharacterCreator Character;

    Transform mPMRootTranform;
    List<IKTargetUI> mIKTargetUIList;

    bool mPaused = true;

    public void Clicked() {
        mPaused = !mPaused;

        if (mPaused) {
            Time.timeScale = 0.0f;
            Character.SetSkeletonAnglesFromPM();
            Character.ShowUI();
        } else {
            Character.SetPMTargetAnglesFromSkeleton();
            ChainLineRenderer.HideAll();
            Character.HideUI();
            Time.timeScale = 1.0f;
        }
    }

    void Start() {
        Physics2D.gravity = new Vector2(0, -3);

        TargetFollower targetFollower = MainCamera.AddComponent<TargetFollower>();
        targetFollower.Target = Character.physicsModel.GetObjectByName("Pelvis").GetComponent<Transform>();
        targetFollower.zDistance = -10;
        targetFollower.Speed = 0.1f;

        Time.timeScale = 0.0f;
    }
}
