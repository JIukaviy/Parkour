using UnityEngine;
using System.Collections.Generic;
using System;

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
    public GameObject LeftFootPrefab;
    public GameObject RightHipPrefab;
    public GameObject RightElbowPrefab;
    public GameObject RightFootPrefab;

    public GameObject IKTargetPrefab;
    public Transform CanvasTransform;

    Skeleton mSkeleton;
    PhysicsModel mPhysicsModel;
    PhysicsModel.SkeletonToPhysicsModelAnglesMap mSkeletonToPMMap;

    IK mLeftArmIK;
    IK mRightArmIK;
    IK mSpineIK;
    IK mLeftLegIK;
    IK mRightLegIK;

    IK.IKTarget mSpine3IKTarget;
    IK.IKTarget mLeftHandIKTarget;
    IK.IKTarget mRightHandIKTarget;
    IK.IKTarget mPelvisIKTarget;
    IK.IKTarget mLeftLegIKTarget;
    IK.IKTarget mRightLegIKTarget;

    IKTargetUI mSpine3IKTargetUI;
    IKTargetUI mLeftHandIKTargetUI;
    IKTargetUI mRightHandIKTargetUI;
    IKTargetUI mLeftLegIKTargetUI;
    IKTargetUI mRightLegIKTargetUI;

    List<IKTargetUI> mIKTargetUIList;

    Transform mPMRootTranform;

    HandGrabber mLeftHand;
    HandGrabber mRightHand;

    public Skeleton skeleton { get { return mSkeleton; } }
    public PhysicsModel physicsModel { get { return mPhysicsModel; } }
    public IK.IKTarget leftHandIKTarget { get { return mLeftHandIKTarget; } }
    public IK.IKTarget rightHandIKTarget { get { return mRightHandIKTarget; } }
    public IK.IKTarget spineIKTarget { get { return mSpine3IKTarget; } }
    public IK.IKTarget pelvisIKTarget { get { return mPelvisIKTarget; } }
    public IK.IKTarget leftLegIKTarget { get { return mLeftLegIKTarget; } }
    public IK.IKTarget rightLegIKTarget { get { return mRightLegIKTarget; } }

    public IKTargetUI leftHandIKTargetUI { get { return mLeftHandIKTargetUI; } }
    public IKTargetUI rightHandIKTargetUI { get { return mRightHandIKTargetUI; } }
    public IKTargetUI spineIKTargetUI { get { return mSpine3IKTargetUI; } }
    public IKTargetUI leftLegIKTargetUI { get { return mLeftLegIKTargetUI; } }
    public IKTargetUI rightLegIKTargetUI { get { return mRightLegIKTargetUI; } }

    public HandGrabber leftHand { get { return mLeftHand; } }
    public HandGrabber rightHand { get { return mRightHand; } }

    public EventHandler OnIKTargetUIPosChanged;


    void AddModelInfo(string Name, ref Dictionary<string, PhysicsModel.PMObjectSettings> PMObjectSettings, 
        ref Dictionary<string, IK.AngleLimits> AngleLimits, float lowerAngle, float upperAngle, Skeleton Sk,
        out Skeleton.Bone Bone, Skeleton.Bone ParentBone, float Length, float OffsetAngle, GameObject Prefab, float Mass, int Layer, string Tag) 
    {
        PMObjectSettings[Name] = new PhysicsModel.PMObjectSettings(lowerAngle, upperAngle, Prefab, Mass, Name, Layer, Tag);
        AngleLimits[Name] = new IK.AngleLimits(lowerAngle, upperAngle);
        Bone = Sk.AddBone(ParentBone, Length, OffsetAngle, Name);
    }

    void AddModelInfo(string Name, ref Dictionary<string, PhysicsModel.PMObjectSettings> PMObjectSettings,
        ref Dictionary<string, IK.AngleLimits> AngleLimits, Skeleton Sk,
        out Skeleton.Bone Bone, Skeleton.Bone ParentBone, float Length, float OffsetAngle, GameObject Prefab, float Mass, int Layer, string Tag) 
    {
        PMObjectSettings[Name] = new PhysicsModel.PMObjectSettings(Prefab, Mass, Name, Layer, Tag); 
        AngleLimits[Name] = null;
        Bone = Sk.AddBone(ParentBone, Length, OffsetAngle, Name);
    }

    void GetModelInfo(float Mass, int Layer, string Tag, out Dictionary<string, IK.AngleLimits> AngleLimits, 
        out Skeleton Sk, out Dictionary<string, PhysicsModel.PMObjectSettings> PMObjectSettings) 
    {
        PMObjectSettings = new Dictionary<string, PhysicsModel.PMObjectSettings>();
        AngleLimits = new Dictionary<string, IK.AngleLimits>();
        Sk = new Skeleton();

        Skeleton.Bone PelvisBone;

        Skeleton.Bone Spine1Bone;
        Skeleton.Bone Spine2Bone;
        Skeleton.Bone Spine3Bone;

        Skeleton.Bone HeadBone;

        Skeleton.Bone LeftShoulderBone;
        Skeleton.Bone LeftArmBone;
        Skeleton.Bone LeftHandBone;

        Skeleton.Bone RightShoulderBone;
        Skeleton.Bone RightArmBone;
        Skeleton.Bone RightHandBone;

        Skeleton.Bone LeftHipBone;
        Skeleton.Bone LeftElbowBone;
        Skeleton.Bone LeftFootBone;

        Skeleton.Bone RightHipBone;
        Skeleton.Bone RightElbowBone;
        Skeleton.Bone RightFootBone;

        AddModelInfo("Pelvis", ref PMObjectSettings, ref AngleLimits, Sk, out PelvisBone, null, 0, 90, PelvisPrefab, Mass * 0.1075f, Layer, Tag);

        AddModelInfo("Spine1", ref PMObjectSettings, ref AngleLimits, -20, 20, Sk, out Spine1Bone, PelvisBone, 0.23f, 0, Spine1Prefab, Mass * 0.1075f, Layer, Tag);
        AddModelInfo("Spine2", ref PMObjectSettings, ref AngleLimits, -20, 20, Sk, out Spine2Bone, Spine1Bone, 0.23f, 0, Spine2Prefab, Mass * 0.1075f, Layer, Tag);
        AddModelInfo("Spine3", ref PMObjectSettings, ref AngleLimits, -20, 20, Sk, out Spine3Bone, Spine2Bone, 0.23f, 0, Spine3Prefab, Mass * 0.1075f, Layer, Tag);

        AddModelInfo("Head", ref PMObjectSettings, ref AngleLimits, -20, 20, Sk, out HeadBone, Spine3Bone, 0.23f, 0, HeadPrefab, Mass * 0.07f, Layer, Tag);

        AddModelInfo("LeftShoulder", ref PMObjectSettings, ref AngleLimits, -130, 160, Sk, out LeftShoulderBone, Spine3Bone, 0.5f, -180, LeftShoulderPrefab, Mass * 0.03f, Layer, Tag);
        AddModelInfo("LeftArm", ref PMObjectSettings, ref AngleLimits, 0, 170, Sk, out LeftArmBone, LeftShoulderBone, 0.5f, 0, LeftArmPrefab, Mass * 0.03f, Layer, Tag);
        AddModelInfo("LeftHand", ref PMObjectSettings, ref AngleLimits, -20, 20, Sk, out LeftHandBone, LeftArmBone, 0.15f, 0, LeftHandPrefab, Mass * 0.01f, Layer, Tag);

        AddModelInfo("RightShoulder", ref PMObjectSettings, ref AngleLimits, -130, 160, Sk, out RightShoulderBone, Spine3Bone, 0.5f, -180, RightShoulderPrefab, Mass * 0.03f, Layer, Tag);
        AddModelInfo("RightArm", ref PMObjectSettings, ref AngleLimits, 0, 170, Sk, out RightArmBone, RightShoulderBone, 0.5f, 0, RightArmPrefab, Mass * 0.03f, Layer, Tag);
        AddModelInfo("RightHand", ref PMObjectSettings, ref AngleLimits, -20, 20, Sk, out RightHandBone, RightArmBone, 0.15f, 0, RightHandPrefab, Mass * 0.01f, Layer, Tag);

        AddModelInfo("LeftHip", ref PMObjectSettings, ref AngleLimits, -80, 150, Sk, out LeftHipBone, PelvisBone, 0.5f, -180, LeftHipPrefab, Mass * 0.12f, Layer, Tag);
        AddModelInfo("LeftElbow", ref PMObjectSettings, ref AngleLimits, -170, 0, Sk, out LeftElbowBone, LeftHipBone, 0.5f, 0, LeftElbowPrefab, Mass * 0.05f, Layer, Tag);
        AddModelInfo("LeftFoot", ref PMObjectSettings, ref AngleLimits, -70, 40, Sk, out LeftFootBone, LeftElbowBone, 0.2f, 90, LeftFootPrefab, Mass * 0.02f, Layer, Tag);

        AddModelInfo("RightHip", ref PMObjectSettings, ref AngleLimits, -80, 150, Sk, out RightHipBone, PelvisBone, 0.5f, -180, RightHipPrefab, Mass * 0.12f, Layer, Tag);
        AddModelInfo("RightElbow", ref PMObjectSettings, ref AngleLimits, -170, 0, Sk, out RightElbowBone, RightHipBone, 0.5f, 0, RightElbowPrefab, Mass * 0.05f, Layer, Tag);
        AddModelInfo("RightFoot", ref PMObjectSettings, ref AngleLimits, -70, 40, Sk, out RightFootBone, RightElbowBone, 0.2f, 90, RightFootPrefab, Mass * 0.02f, Layer, Tag);
    }

    IKTargetUI CreateIKTargetUI(IK.IKTarget Target) {
        GameObject gameObject = Instantiate(IKTargetPrefab);
        IKTargetUI targetUI = gameObject.GetComponent<IKTargetUI>();
        targetUI.IKTarget = Target;
        targetUI.skeleton = mSkeleton;
        targetUI.OnPosChanged += OnTargetPosChanged;
        gameObject.transform.SetParent(CanvasTransform, false);
        mIKTargetUIList.Add(targetUI);
        return targetUI;
    }

    public void ShowUI() {
        foreach(IKTargetUI targetUI in mIKTargetUIList) {
            targetUI.Show();
        }
        ChainLineRenderer.ShowAll();
    }

    public void HideUI() {
        foreach(IKTargetUI targetUI in mIKTargetUIList) {
            targetUI.Hide();
        }
        ChainLineRenderer.HideAll();
    }

    public void OnTargetPosChanged(object sender, EventArgs e) {
        if (OnIKTargetUIPosChanged != null) {
            OnIKTargetUIPosChanged.Invoke(sender, e);
        }
    }

    void Start() {
        Physics2D.gravity = new Vector2(0, -3);

        Dictionary<string, IK.AngleLimits> AngleLimits;
        Dictionary<string, PhysicsModel.PMObjectSettings> PMObjectSettings;

        GetModelInfo(0.01f, LayerMask.NameToLayer("Character"), "Dynamic", out AngleLimits, out mSkeleton, out PMObjectSettings);

        PhysicsModel.PhysicsModelFromSkeletonCreator PhysicsModelFromSkeletonCreator = 
            new PhysicsModel.PhysicsModelFromSkeletonCreator(mSkeleton.root, PMObjectSettings);
        mPhysicsModel = PhysicsModelFromSkeletonCreator.GetPhysicsModel();

        mSpine3IKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("Spine3").endPoint);
        mLeftHandIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("LeftHand").endPoint);
        mRightHandIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("RightHand").endPoint);
        mPelvisIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("Pelvis").endPoint);
        mLeftLegIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("LeftElbow").endPoint);
        mRightLegIKTarget = new IK.IKTarget(mSkeleton.GetBoneByName("RightElbow").endPoint);

        mLeftArmIK = new IK(mSkeleton, "Spine3", "LeftHand", AngleLimits, mSpine3IKTarget, mLeftHandIKTarget);
        mRightArmIK = new IK(mSkeleton, "Spine3", "RightHand", AngleLimits, mSpine3IKTarget, mRightHandIKTarget);
        mSpineIK = new IK(mSkeleton, "Pelvis", "Spine3", AngleLimits, mPelvisIKTarget, mSpine3IKTarget);
        mRightLegIK = new IK(mSkeleton, "Pelvis", "RightFoot", AngleLimits, mPelvisIKTarget, mRightLegIKTarget);
        mLeftLegIK = new IK(mSkeleton, "Pelvis", "LeftFoot", AngleLimits, mPelvisIKTarget, mLeftLegIKTarget);

        mSpine3IKTarget.anchor = false;

        ChainLineRenderer.Init();
        ChainLineRenderer.ApplyLineRenderer(mSkeleton.root, 0.05f);

        mSkeletonToPMMap = new PhysicsModel.SkeletonToPhysicsModelAnglesMap(mSkeleton.GetLocalAnglesOrder(), mPhysicsModel.GetAnglesOrder());

        mPMRootTranform = mPhysicsModel.GetObjectByName("Pelvis").transform;

        mIKTargetUIList = new List<IKTargetUI>();

        mLeftHandIKTargetUI = CreateIKTargetUI(mLeftHandIKTarget);
        mRightHandIKTargetUI = CreateIKTargetUI(mRightHandIKTarget);
        mSpine3IKTargetUI = CreateIKTargetUI(mSpine3IKTarget);
        mLeftLegIKTargetUI = CreateIKTargetUI(mLeftLegIKTarget);
        mRightLegIKTargetUI = CreateIKTargetUI(mRightLegIKTarget);

        mLeftHand = mPhysicsModel.GetObjectByName("LeftHand").GetComponent<HandGrabber>();
        mRightHand = mPhysicsModel.GetObjectByName("RightHand").GetComponent<HandGrabber>();

        mPelvisIKTarget.UpdatePosition();
    }

    public void SetPMTargetAnglesFromSkeleton() {
        mPhysicsModel.SetTargetAngles(mSkeletonToPMMap.ConvertSkToPmAngles(mSkeleton.GetLocalAngles()));
    }

    public void SetSkeletonAnglesFromPM() {
        mSkeleton.root.startPoint = mPMRootTranform.position;
        mSkeleton.root.worldAngle = new Quaternion2D(mPMRootTranform.rotation.eulerAngles.z);
        mSkeleton.SetLocalAngles(mSkeletonToPMMap.ConvertPmToSkAngles(mPhysicsModel.GetAngles()));
        mPelvisIKTarget.UpdatePosition();
    }
}
