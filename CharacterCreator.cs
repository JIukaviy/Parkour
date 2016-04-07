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
        res["Pelvis"]        = null;

        res["Spine1"]        = new IK.AngleLimits(-20, 20);
        res["Spine2"]        = new IK.AngleLimits(-20, 20);
        res["Spine3"]        = new IK.AngleLimits(-20, 20);

        res["Head"]          = new IK.AngleLimits(-20, 20);

        res["LeftShoulder"]  = new IK.AngleLimits(-90, 180);
        res["LeftArm"]       = new IK.AngleLimits(0, 170);
        res["LeftHand"]      = new IK.AngleLimits(-20, 20);

        res["RightShoulder"] = new IK.AngleLimits(-90, 180);
        res["RightArm"]      = new IK.AngleLimits(0, 170);
        res["RightHand"]     = new IK.AngleLimits(-20, 20);

        res["LeftHip"]       = new IK.AngleLimits(-30, 170);
        res["LeftElbow"]     = new IK.AngleLimits(-170, 0);

        res["RightHip"]      = new IK.AngleLimits(-30, 170);
        res["RightElbow"]    = new IK.AngleLimits(-170, 0);

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

    void Start () {
        Skeleton.BipedSkeletonCreator BipedSkeletonCreator = new Skeleton.BipedSkeletonCreator();
        mSkeleton = BipedSkeletonCreator.GetSkeleton();

        Dictionary<string, IK.AngleLimits> AngleLimits = GetAngleLimits();
        Dictionary<string, GameObject> Prefabs = GetPrefabs();

        PhysicsModel.PhysicsModelFromSkeletonCreator PhysicsModelFromSkeletonCreator = 
            new PhysicsModel.PhysicsModelFromSkeletonCreator(mSkeleton.root, AngleLimits, Prefabs, LayerMask.NameToLayer("Character"));
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

        /*IK.IKChainFromSkeletonCreator IKChainFromSkeletonCreator =
            new IK.IKChainFromSkeletonCreator(mSkeleton.root, AngleLimits);
        mIK = IKChainFromSkeletonCreator.GetIKChain();

        mIKHand = mIK.FindByName("LeftHand");*/

        ChainLineRenderer.ApplyLineRenderer(mSkeleton.root, 0.05f);

        mSkeletonToPMMap = new PhysicsModel.SkeletonToPhysicsModelAnglesMap(mSkeleton.GetLocalAnglesOrder(), mPhysicsModel.GetAnglesOrder(), 
            mSkeleton.GetLocalAngles(), mPhysicsModel.GetTargetAngles());

        mPhysicsModel.SetAngleLimits(mSkeletonToPMMap.ConvertAngleLimits(AngleLimits));
    }

    void Update() {
        mLeftHandIKTarget.TryMoveTo(LeftHandTargetObject.position);
        mRightHandIKTarget.TryMoveTo(RightHandTargetObject.position);
        //mSpine3IKTarget.TryMoveTo(Spine3TargetObject.position);
        mLeftElbowIKTarget.TryMoveTo(LeftLegTargetObject.position);
        mRightElbowIKTarget.TryMoveTo(RightLegTargetObject.position);
        mSkeleton.UpdateSkeleton();
        mPhysicsModel.SetTargetAngles(mSkeletonToPMMap.ConvertAngles(mSkeleton.GetLocalAngles()));
    }
}
