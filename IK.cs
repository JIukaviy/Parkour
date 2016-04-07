using UnityEngine;
using System.Collections.Generic;
using System;

public class IK {
    public class BoneNotFindedException : ArgumentException {
        static string GetMessage(string BoneName) {
            return "Bone " + BoneName + " not finded";
        }

        public BoneNotFindedException(string BoneName) : base(GetMessage(BoneName)) { }
        public BoneNotFindedException(string BoneName, Exception InnerException) : base(GetMessage(BoneName), InnerException) { }
    }

    public class BoneIsNotChildOfException : ArgumentException {
        static string GetMessage(string ParentBoneName, string ChildBoneName) {
            return "Bone " + ParentBoneName + " is not child of " + ChildBoneName;
        }

        public BoneIsNotChildOfException(string ParentBoneName, string ChildBoneName) : base(GetMessage(ParentBoneName, ChildBoneName)) { }
        public BoneIsNotChildOfException(string ParentBoneName, string ChildBoneName, Exception InnerException) : base(GetMessage(ParentBoneName, ChildBoneName), InnerException) { }
    }

    public class AngleLimits {
        Quaternion2D mMiddle;
        Quaternion2D mMiddleNormal;
        Quaternion2D mMaxAngle;
        Quaternion2D mMinAngle;
        float mCosLimit;

        public Quaternion2D minAngle {
            get {
                return mMinAngle;
            }
        }

        public Quaternion2D maxAngle {
            get {
                return mMaxAngle;
            }
        }

        public bool isFixed {
            get {
                return mMaxAngle == mMinAngle;
            }
        }

        public AngleLimits(float MinAngle, float MaxAngle) {
            mMinAngle = new Quaternion2D(MinAngle);
            mMaxAngle = new Quaternion2D(MaxAngle);
            mMiddle = Quaternion2D.MiddleBetween(mMinAngle, mMaxAngle);
            mMiddleNormal = mMiddle.GetNormal();
            mCosLimit = Quaternion2D.CosBetween(mMiddle, mMaxAngle);
        }

        public JointAngleLimits2D ToJointAngleLimits2D() {
            JointAngleLimits2D res = new JointAngleLimits2D();
            res.max = mMaxAngle.euler;
            res.min = mMinAngle.euler;
            return res;
        }

        public Quaternion2D ChainAngle(Quaternion2D Angle) {
            float cos = Quaternion2D.CosBetween(mMiddle, Angle);
            if (cos > mCosLimit) {
                return Angle;
            } else {
                return Quaternion2D.CosBetween(mMiddleNormal, Angle) > 0 ? mMaxAngle : mMinAngle;
            }
        }
    }

    public class IKTarget {
        public class IKTargetID {
            IKTarget mIKTarget;
            int mIKID;

            public IKTargetID(IKTarget Target, int ID) {
                mIKTarget = Target;
                mIKID = ID;
            }

            public Vector2 TryMoveTo(Vector2 Target) {
                return mIKTarget.IKTryMoveTo(Target, mIKID);
            }

            public void MoveTo(Vector2 Position) {
                mIKTarget.MoveTo(Position, mIKID);
            }
        }

        struct IKAndID {
            public IK ik;
            public int id;

            public IKAndID(IK Ik, int Id) {
                ik = Ik;
                id = Id;
            }
        }

        Vector2 mPosition;
        bool mAnchor;
        List<IKAndID> mBackwardIK;
        List<IKAndID> mForwardIK;
        int mIKCount;

        public bool anchor {
            get {
                return mAnchor;
            }
            set {
                mAnchor = value;
            }
        }

        public Vector2 position {
            get {
                return mPosition;
            }
        }

        public IKTarget(Vector2 Position) {
            mPosition = Position;
            mBackwardIK = new List<IKAndID>();
            mForwardIK = new List<IKAndID>();
            mAnchor = true;
            mIKCount = 0;
        }

        public IKTargetID AddForwardIK(IK Ik) {
            mForwardIK.Add(new IKAndID(Ik, mIKCount));
            return new IKTargetID(this, mIKCount++);
        }

        public IKTargetID AddBackwardIK(IK Ik) {
            mBackwardIK.Add(new IKAndID(Ik, mIKCount));
            return new IKTargetID(this, mIKCount++);
        }
        
        Vector2 TryMoveTo(Vector2 Target, int IkId) {
            foreach(IKAndID ikAndId in mBackwardIK) {
                if (ikAndId.id != IkId) {
                    Target = ikAndId.ik.TryMoveEndTo(Target);
                }
            }
            foreach (IKAndID ikAndId in mForwardIK) {
                if (ikAndId.id != IkId) {
                    Target = ikAndId.ik.TryMoveStartTo(Target);
                }
            }
            mPosition = Target;
            return mPosition;
        }

        Vector2 IKTryMoveTo(Vector2 Target, int IkId) {
            return mAnchor ? mPosition : TryMoveTo(Target, IkId);
        }

        public Vector2 TryMoveTo(Vector2 Target) {
            return TryMoveTo(Target, -1);
        }

        void MoveTo(Vector2 Position, int IkId = -1) {
            mPosition = Position;

            foreach (IKAndID ikAndId in mBackwardIK) {
                if (ikAndId.id != IkId) {
                    ikAndId.ik.UpdateTargetPositions();
                }
            }
            foreach (IKAndID ikAndId in mForwardIK) {
                if (ikAndId.id != IkId) {
                    ikAndId.ik.UpdateTargetPositions();
                }
            }
        }

        public void UpdatePosition() {
            Vector2 middlePos = new Vector2();

            foreach (IKAndID ikAndId in mBackwardIK) {
                middlePos += ikAndId.ik.endPoint;
            }
            foreach (IKAndID ikAndId in mForwardIK) {
                middlePos += ikAndId.ik.startPoint;
            }

            middlePos /= mBackwardIK.Count + mForwardIK.Count;

            MoveTo(middlePos);
        }
    }
    
    Skeleton mSkeleton;
    Skeleton.Bone[] mBones;
    AngleLimits[] mAngleLimits;
    IKTarget.IKTargetID mEndTarget;
    IKTarget.IKTargetID mStartTarget;

    public Vector2 startPoint { get { return mBones[mBones.Length - 1].startPoint; } }
    public Vector2 endPoint { get { return mBones[0].endPoint; } }

    public IK(Skeleton skeleton, string startBoneName, string endBoneName, Dictionary<string, AngleLimits> AngleLimits, IKTarget StartTarget, IKTarget EndTarget) {
        mSkeleton = skeleton;

        Skeleton.Bone endBone = skeleton.GetBoneByName(endBoneName);
        if (endBone == null) {
            throw new BoneNotFindedException(endBoneName);
        }

        List<Skeleton.Bone> bones = GetBoneChain(endBone, startBoneName);
        if (bones == null) {
            throw new BoneIsNotChildOfException(startBoneName, endBoneName);
        }

        mBones = bones.ToArray();
        mAngleLimits = new AngleLimits[mBones.Length];
        for (int i = 0; i < mBones.Length; i++) {
            mAngleLimits[i] = AngleLimits[mBones[i].name];
        }

        mStartTarget = StartTarget.AddForwardIK(this);
        mEndTarget = EndTarget.AddBackwardIK(this);
    }

    public List<Skeleton.Bone> GetBoneChain(Skeleton.Bone Bone, string Name) {
        List<Skeleton.Bone> res = new List<Skeleton.Bone>();
        while (true) {
            res.Add(Bone);
            Bone = Bone.parent;
            if (Bone == null) {
                return null;
            } else if (Bone.name == Name) {
                res.Add(Bone);
                return res;
            }
        }
    }

    void LookEndPointTo(Skeleton.Bone Bone, AngleLimits Limits, Vector2 Target) {
        Quaternion2D angle = Bone.WorldAngleToLocal(new Quaternion2D(Bone.startPoint, Target));
        
        if (Limits != null) {
            angle = Limits.ChainAngle(angle);
        }

        Bone.localAngle = angle;
    }

    void LookStartPointTo(Skeleton.Bone Bone, AngleLimits Limits, Vector2 Target) {
        Quaternion2D angle = Bone.WorldAngleToLocal(new Quaternion2D(Target, Bone.endPoint));

        if (Limits != null) {
            angle = Limits.ChainAngle(angle);
        }

        Bone.localAngle = angle;
    }

    Vector2 BackwardStep(Vector2 Target) {
        for (int i = 0; i < mBones.Length - 1; i++) {
            Skeleton.Bone bone = mBones[i];
            LookEndPointTo(bone, mAngleLimits[i], Target);
            bone.endPoint = Target;
            Target = bone.startPoint;
        }
        return Target;
    }

    Vector2 ForwardStep(Vector2 Target) {
        for (int i = mBones.Length - 2; i >= 0; i--) {
            Skeleton.Bone bone = mBones[i];
            LookStartPointTo(bone, mAngleLimits[i], Target);
            bone.startPoint = Target;
            Target = bone.endPoint;
        }
        return Target;
    }

    public Vector2 TryMoveEndTo(Vector2 Target) {
        Target = BackwardStep(Target);
        Target = mStartTarget.TryMoveTo(Target);
        Target = ForwardStep(Target);
        return Target;
    }

    public Vector2 TryMoveStartTo(Vector2 Target) {
        Target = ForwardStep(Target);
        Target = mEndTarget.TryMoveTo(Target);
        Target = BackwardStep(Target);
        return Target;
    }

    void UpdateTargetPositions() {
        mEndTarget.MoveTo(mBones[mBones.Length - 1].endPoint);
    }
}
