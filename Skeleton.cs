using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Skeleton {
    public class Bone {
        Bone mParent;
        List<Bone> mChilds;
        Vector2 mStartPoint;
        Vector2 mEndPoint;
        float mLength;
        float mAngle;
        float mLocalAngle;
        float mOffsetAngle;
        string mName;

        public float WorldAngleToLocal(float Angle) {
            return Angle - (mParent != null ? mParent.angle : 0) - mOffsetAngle;
        }

        public float LocalAngleToWorld(float Angle) {
            return (mParent != null ? mParent.angle : 0) + mOffsetAngle + Angle;
        }

        void UpdateAngle() {
            mAngle = LocalAngleToWorld(mLocalAngle);
        }

        void UpdateEndPoint() {
            mEndPoint = mStartPoint + new Vector2(Mathf.Cos(Mathf.Deg2Rad * mAngle), Mathf.Sin(Mathf.Deg2Rad * mAngle)) * mLength;
        }

        public float angle {
            get {
                return mAngle;
            }
            set {
                mAngle = value;
                UpdateEndPoint();
            }
        }

        public float localAngle {
            get {
                return mLocalAngle;
            }
            set {
                mLocalAngle = value;
                UpdateAngle();
                UpdateEndPoint();
            }
        }

        public float offsetAngle {
            get {
                return mOffsetAngle;
            }
            set {
                mAngle += value - mOffsetAngle;
                mOffsetAngle = value;
                UpdateEndPoint();
            }
        }

        public float length {
            get {
                return mLength;
            }
            set {
                mLength = value;
                UpdateEndPoint();
            }
        }

        public string name {
            get {
                return mName;
            }
        }

        public List<Bone> childs {
            get {
                return mChilds;
            }
        }

        public Bone parent {
            get {
                return mParent;
            }
        }

        public Vector2 startPoint {
            get {
                return mStartPoint;
            }
            set {
                mEndPoint += value - endPoint;
                mStartPoint = value;
            }
        }

        public Vector2 endPoint {
            get {
                return mEndPoint;
            }
            set {
                mStartPoint += value - endPoint;
                mEndPoint = value;
            }
        }

        public Bone(Bone Parent, float Length, float OffsetAngle, string Name) {
            mChilds = new List<Bone>();
            mName = Name;
            mParent = Parent;
            
            if (mParent != null) {
                mParent.AddChild(this);
            }
            
            mLength = Length;
            mOffsetAngle = OffsetAngle;
            mAngle = OffsetAngle;

            UpdateEndPoint();
        }

        public void AddChild(Bone Child) {
            mChilds.Add(Child);
            Child.startPoint = mEndPoint;
        }

        public void UpdateChilds() {
            foreach (Bone bone in mChilds) {
                bone.mStartPoint = mEndPoint;

                bone.UpdateAngle();
                bone.UpdateEndPoint();
                bone.UpdateChilds();
            }
        }

        public Bone FindByName(string Name) {
            if (mName == Name) {
                return this;
            } else {
                foreach (Bone child in mChilds) {
                    Bone findedChild = child.FindByName(Name);
                    if (findedChild != null) {
                        return findedChild;
                    }
                }
            }
            return null;
        }
    }

    Bone mRoot;
    List<Bone> mBones;
    Dictionary<string, int> mNameToId;

    public Bone root {
        get {
            return mRoot;
        }
    }

    public Skeleton() {
        mBones = new List<Bone>();
        mNameToId = new Dictionary<string, int>();
    }

    public Bone AddBone(Bone Parent, float Length, float OffsetAngle, string Name) {
        Bone newBone = new Bone(Parent, Length, OffsetAngle, Name);
        mBones.Add(newBone);
        mNameToId[Name] = mBones.Count - 1;

        if (Parent == null) {
            mRoot = newBone;
        }

        return newBone;
    }

    public void UpdateSkeleton() {
        mRoot.UpdateChilds();
    }

    public Bone GetBoneByName(string Name) {
        return mBones[mNameToId[Name]];
    }

    public Bone[] GetBones() {
        return mBones.ToArray();
    }

    public Dictionary<string, int> GetLocalAnglesOrder() {
        Dictionary<string, int> localAnglesOrder = new Dictionary<string, int>();

        foreach (KeyValuePair<string, int> kv in mNameToId) {
            if (kv.Key != mRoot.name) {
                localAnglesOrder[kv.Key] = kv.Value - 1;
            }
        }

        return localAnglesOrder;
    }

    public float[] GetAngles() {
        float[] res = new float[mBones.Count];

        for (int i = 0; i < mBones.Count; i++) {
            res[i] = mBones[i].localAngle;
        }

        return res;
    }

    public float[] GetLocalAngles() {
        float[] res = new float[mBones.Count - 1];

        for (int i = 0; i < mBones.Count - 1; i++) {
            res[i] = mBones[i + 1].localAngle;
        }

        return res;
    }

    public float[] GetLocalAnglesWithoutOffset() {
        float[] res = new float[mBones.Count - 1];

        for (int i = 0; i < mBones.Count - 1; i++) {
            res[i] = mBones[i + 1].localAngle - mBones[i + 1].offsetAngle;
        }

        return res;
    }

    interface SkeletonCreator {
        Skeleton GetSkeleton();
    }

    public class BipedSkeletonCreator : SkeletonCreator {
        public Skeleton GetSkeleton() {
            Skeleton skeleton = new Skeleton();

            Bone pelvis = skeleton.AddBone(null, 0, 90, "Pelvis");

            Bone spine1 = skeleton.AddBone(pelvis, 0.23f, 0, "Spine1");
            Bone spine2 = skeleton.AddBone(spine1, 0.23f, 0, "Spine2");
            Bone spine3 = skeleton.AddBone(spine2, 0.23f, 0, "Spine3");

            Bone head = skeleton.AddBone(spine3, 0.3f, 0, "Head");

            Bone leftShoulder = skeleton.AddBone(spine3, 0.5f, -180, "LeftShoulder");
            Bone leftArm = skeleton.AddBone(leftShoulder, 0.5f, 0, "LeftArm");
            Bone leftHand = skeleton.AddBone(leftArm, 0.15f, 0, "LeftHand");

            Bone rightShoulder = skeleton.AddBone(spine3, 0.5f, -180, "RightShoulder");
            Bone rightArm = skeleton.AddBone(rightShoulder, 0.5f, 0, "RightArm");
            Bone rightHand = skeleton.AddBone(rightArm, 0.15f, 0, "RightHand");

            Bone leftHip = skeleton.AddBone(pelvis, 0.5f, -180, "LeftHip");
            Bone leftElbow = skeleton.AddBone(leftHip, 0.5f, 0, "LeftElbow");

            Bone rightHip = skeleton.AddBone(pelvis, 0.5f, -180, "RightHip");
            Bone rightElbow = skeleton.AddBone(rightHip, 0.5f, 0, "RightElbow");

            skeleton.UpdateSkeleton();
            return skeleton;
        }
    }

    static public int GetBoneCount(Bone Parent) {
        int count = 1;
        foreach (Bone bone in Parent.childs) {
            count += GetBoneCount(bone);
        }
        return count;
    }

    static void FlattenSkeleton(Bone Root, ref int Id, ref Bone[] Output) {
        Output[Id++] = Root;
        foreach (Bone bone in Root.childs) {
            FlattenSkeleton(bone, ref Id, ref Output);
        }
    }

    static public Bone[] FlattenSkeleton(Bone Root) {
        Bone[] res = new Bone[GetBoneCount(Root)];
        int id = 0;
        FlattenSkeleton(Root, ref id, ref res);
        return res;
    }
}
