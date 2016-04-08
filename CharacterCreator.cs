using UnityEngine;
using System.Collections.Generic;

public class CharacterCreator : MonoBehaviour {
    public GameObject PelvisPrefab;
    public GameObject Spine1Prefab;
    public GameObject Spine2Prefab;
    public GameObject Spine3Prefab;
    public GameObject LeftShoulderPrefab;
    public GameObject LeftArmPrefab;
    public GameObject LeftHandPrefab;
    public GameObject RightShoulderPrefab;
    public GameObject RightArmPrefab;
    public GameObject RightHandPrefab;
    public GameObject HeadPrefab;
    public GameObject LeftHipPrefab;
    public GameObject LeftElbowPrefab;
    public GameObject RightHipPrefab;
    public GameObject RightElbowPrefab;

    public Transform LeftHandTargetObject;
    public Transform RightHandTargetObject;
    public Transform Spine3TargetObject;
    public Transform LeftLegTargetObject;
    public Transform RightLegTargetObject;

    Skeleton mSkeleton;
    IK mLeftArmIK;
    IK mRightArmIK;
    IK mSpineIK;
    IK mLeftLegIK;
    IK mRightLegIK;
    IK.IKTarget mSpine3IKTarget;
    IK.IKTarget mLeftHandIKTarget;
    IK.IKTarget mRightHandIKTarget;
    IK.IKTarget mPelvisIKTarget;
    IK.IKTarget mLeftElbowIKTarget;
    IK.IKTarget mRightElbowIKTarget;
    PhysicsModel.PhysicsModel mPhysicsModel;
    PhysicsModel.SkeletonToPhysicsModelAnglesMap mSkeletonToPMMap;
    Transform mPMRootTranform;

    public Skeleton skeleton { get { return mSkeleton; } }
    public IK.IKTarget leftArmIKTarget { get { return mLeftHandIKTarget; } }
    public IK.IKTarget rightArmIKTaret { get { return mRightHandIKTarget; } }
    public IK.IKTarget spineIKTarget { get { return mSpine3IKTarget; } }
    public IK.IKTarget pelvisIKTarget { get { return mPelvisIKTarget; } }
    public IK.IKTarget leftElbowIKTarget { get { return mLeftElbowIKTarget; } }
    public IK.IKTarget rightElbowIKTarget { get { return mRightElbowIKTarget; } }

    Skeleton GetSkeleton() {
        Skeleton skeleton = new Skeleton();

        Skeleton.Bone pelvis = skeleton.AddBone(null, 0, 90, "Pelvis");

        Skeleton.Bone spine1 = skeleton.AddBone(pelvis, 0.23f, 0, "Spine1");
        Skeleton.Bone spine2 = skeleton.AddBone(spine1, 0.23f, 0, "Spine2");
        Skeleton.Bone spine3 = skeleton.AddBone(spine2, 0.23f, 0, "Spine3");

        Skeleton.Bone head = skeleton.AddBone(spine3, 0.3f, 0, "Head");

        Skeleton.Bone leftShoulder = skeleton.AddBone(spine3, 0.5f, -180, "LeftShoulder");
        Skeleton.Bone leftArm = skeleton.AddBone(leftShoulder, 0.5f, 0, "LeftArm");
        Skeleton.Bone leftHand = skeleton.AddBone(leftArm, 0.15f, 0, "LeftHand");

        Skeleton.Bone rightShoulder = skeleton.AddBone(spine3, 0.5f, -180, "RightShoulder");
        Skeleton.Bone rightArm = skeleton.AddBone(rightShoulder, 0.5f, 0, "RightArm");
        Skeleton.Bone rightHand = skeleton.AddBone(rightArm, 0.15f, 0, "RightHand");

        Skeleton.Bone leftHip = skeleton.AddBone(pelvis, 0.5f, -180, "LeftHip");
        Skeleton.Bone leftElbow = skeleton.AddBone(leftHip, 0.5f, 0, "LeftElbow");

        Skeleton.Bone rightHip = skeleton.AddBone(pelvis, 0.5f, -180, "RightHip");
        Skeleton.Bone rightElbow = skeleton.AddBone(rightHip, 0.5f, 0, "RightElbow");

        skeleton.UpdateSkeleton();
        return skeleton;
    }

    Dictionary<string, IK.AngleLimits> GetAngleLimits() {
        Dictionary<string, IK.AngleLimits> res = new Dictionary<string, IK.AngleLimits>();
        res["Pelvis"] = null;

        res["Spine1"] = new IK.AngleLimits(0, 0);
        res["Spine2"] = new IK.AngleLimits(-20, 20);
        res["Spine3"] = new IK.AngleLimits(-20, 20);

        res["Head"] = new IK.AngleLimits(-20, 20);

        res["LeftShoulder"] = new IK.AngleLimits(-130, 160);
        res["LeftArm"] = new IK.AngleLimits(0, 170);
        res["LeftHand"] = new IK.AngleLimits(-20, 20);

        res["RightShoulder"] = new IK.AngleLimits(-130, 160);
        res["RightArm"] = new IK.AngleLimits(0, 170);
        res["RightHand"] = new IK.AngleLimits(-20, 20);

        res["LeftHip"] = new IK.AngleLimits(-80, 150);
        res["LeftElbow"] = new IK.AngleLimits(-170, 0);

        res["RightHip"] = new IK.AngleLimits(-80, 150);
        res["RightElbow"] = new IK.AngleLimits(-170, 0);

        return res;
    }

    Dictionary<string, GameObject> GetPrefabs() {
        Dictionary<string, GameObject> res = new Dictionary<string, GameObject>();
        res["Pelvis"] = PelvisPrefab;

        res["Spine1"] = Spine1Prefab;
        res["Spine2"] = Spine2Prefab;
        res["Spine3"] = Spine3Prefab;

        res["Head"] = HeadPrefab;

        res["LeftShoulder"] = LeftShoulderPrefab;
        res["LeftArm"] = LeftArmPrefab;
        res["LeftHand"] = LeftHandPrefab;

        res["RightShoulder"] = RightShoulderPrefab;
        res["RightArm"] = RightArmPrefab;
        res["RightHand"] = RightHandPrefab;

        res["LeftHip"] = LeftHipPrefab;
        res["LeftElbow"] = LeftElbowPrefab;

        res["RightHip"] = RightHipPrefab;
        res["RightElbow"] = RightElbowPrefab;

        return res;
    }

    Dictionary<string, float> GetMasses() {
        Dictionary<string, float> res = new Dictionary<string, float>();

        res["Pelvis"] = 8.17f;

        res["Spine1"] = 8.17f;
        res["Spine2"] = 8.14f;
        res["Spine3"] = 8.17f;

        res["Head"] = 5.32f;

        res["LeftShoulder"] = 2.28f;
        res["LeftArm"] = 1.52f;
        res["LeftHand"] = 0.76f;

        res["RightShoulder"] = 2.28f;
        res["RightArm"] = 1.52f;
        res["RightHand"] = 0.76f;

        res["LeftHip"] = 9.12f;
        res["LeftElbow"] = 3.8f;

        res["RightHip"] = 9.12f;
        res["RightElbow"] = 3.8f;

        return res;
    }

    void Start () {
        mSkeleton = GetSkeleton();

        Dictionary<string, IK.AngleLimits> AngleLimits = GetAngleLimits();
        Dictionary<string, GameObject> Prefabs = GetPrefabs();
        Dictionary<string, float> Masses = GetMasses();

        PhysicsModel.PhysicsModelFromSkeletonCreator PhysicsModelFromSkeletonCreator = 
            new PhysicsModel.PhysicsModelFromSkeletonCreator(mSkeleton.root, AngleLimits, Masses, Prefabs, LayerMask.NameToLayer("Character"));
        mPhysicsModel = PhysicsModelFromSkeletonCreator.GetPhysicsModel();

        mSpine3IKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("Spine3").endPoint);
        mLeftHandIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("LeftHand").endPoint);
        mRightHandIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("RightHand").endPoint);
        mPelvisIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("Pelvis").endPoint);
        mLeftElbowIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("LeftElbow").endPoint);
        mRightElbowIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("RightElbow").endPoint);
        mLeftArmIK = new IK(mSkeleton, "Spine3", "LeftHand", AngleLimits, mSpine3IKTarget, mLeftHandIKTarget);
        mRightArmIK = new IK(mSkeleton, "Spine3", "RightHand", AngleLimits, mSpine3IKTarget, mRightHandIKTarget);
        mSpineIK = new IK(mSkeleton, "Pelvis", "Spine3", AngleLimits, mPelvisIKTarget, mSpine3IKTarget);
        mRightLegIK = new IK(mSkeleton, "Pelvis", "RightElbow", AngleLimits, mPelvisIKTarget, mRightElbowIKTarget);
        mLeftLegIK = new IK(mSkeleton, "Pelvis", "LeftElbow", AngleLimits, mPelvisIKTarget, mLeftElbowIKTarget);

        mSpine3IKTarget.anchor = false;

        ChainLineRenderer.ApplyLineRenderer(mSkeleton.root, 0.05f);

        mSkeletonToPMMap = new PhysicsModel.SkeletonToPhysicsModelAnglesMap(mSkeleton.GetLocalAnglesOrder(), mPhysicsModel.GetAnglesOrder());

        mPMRootTranform = mPhysicsModel.GetObjectByName("Pelvis").transform;

        Time.timeScale = 0.0f;
    }

    bool mPaused = true;

    public void Clicked() {
        mPaused = !mPaused;

        if (mPaused) {
            Time.timeScale = 0.0f;
            mSkeleton.root.startPoint = mPMRootTranform.position;
            mSkeleton.root.worldAngle = new Quaternion2D(mPMRootTranform.rotation.eulerAngles.z);
            mSkeleton.SetLocalAngles(mSkeletonToPMMap.ConvertPmToSkAngles(mPhysicsModel.GetAngles()));
            mPelvisIKTarget.UpdatePosition();

            LeftHandTargetObject.position = mLeftHandIKTarget.position;
            RightHandTargetObject.position = mRightHandIKTarget.position;
            Spine3TargetObject.position = mSpine3IKTarget.position;
            LeftLegTargetObject.position = mLeftElbowIKTarget.position;
            RightLegTargetObject.position = mRightElbowIKTarget.position;
        } else {
            mPhysicsModel.SetTargetAngles(mSkeletonToPMMap.ConvertSkToPmAngles(mSkeleton.GetLocalAngles()));
            Time.timeScale = 1.0f;
        }
    }

    void Update() {
        LeftHandTargetObject.position = mLeftHandIKTarget.TryMoveTo(LeftHandTargetObject.position);
        RightHandTargetObject.position = mRightHandIKTarget.TryMoveTo(RightHandTargetObject.position);
        Spine3TargetObject.position = mSpine3IKTarget.TryMoveTo(Spine3TargetObject.position);
        LeftLegTargetObject.position = mLeftElbowIKTarget.TryMoveTo(LeftLegTargetObject.position);
        RightLegTargetObject.position = mRightElbowIKTarget.TryMoveTo(RightLegTargetObject.position);
        mSkeleton.UpdateSkeleton();

        LeftHandTargetObject.position = mLeftHandIKTarget.position;
        RightHandTargetObject.position = mRightHandIKTarget.position;
        Spine3TargetObject.position = mSpine3IKTarget.position;
        LeftLegTargetObject.position = mLeftElbowIKTarget.position;
        RightLegTargetObject.position = mRightElbowIKTarget.position;
    }
}
