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
        Quaternion2D mWorldAngle;
        Quaternion2D mLocalAngle;
        Quaternion2D mOffsetAngle;
        string mName;

        public Quaternion2D WorldAngleToLocal(Quaternion2D Angle) {
            Quaternion2D res = Angle / mOffsetAngle;
            if (mParent != null) {
                res.InverseRotate(mParent.worldAngle);
            }
            return res;
        }

        public Quaternion2D LocalAngleToWorld(Quaternion2D Angle) {
            return (mParent != null ? mParent.worldAngle : new Quaternion2D()) * mOffsetAngle * Angle;
        }

        void UpdateWorldAngle() {
            mWorldAngle = LocalAngleToWorld(mLocalAngle);
        }

        void UpdateEndPoint() {
            mEndPoint = mStartPoint + mWorldAngle.GetVector(mLength);
        }

        public Quaternion2D worldAngle {
            get { return new Quaternion2D(mWorldAngle); }
            set {
                mWorldAngle = new Quaternion2D(value);
                mLocalAngle = WorldAngleToLocal(value);
                UpdateEndPoint();
            }
        }

        public Quaternion2D localAngle {
            get { return mLocalAngle; }
            set {
                mLocalAngle = new Quaternion2D(value);
                UpdateWorldAngle();
                UpdateEndPoint();
            }
        }

        public Quaternion2D offsetAngle { get { return new Quaternion2D(mOffsetAngle); } }

        public float length {
            get { return mLength; }
            set {
                mLength = value;
                UpdateEndPoint();
            }
        }

        public string name { get { return mName; } }

        public List<Bone> childs { get { return mChilds; } }

        public Bone parent { get { return mParent; } }

        public Vector2 startPoint {
            get { return mStartPoint; }
            set {
                mEndPoint += value - mStartPoint;
                mStartPoint = value;
            }
        }

        public Vector2 endPoint {
            get { return mEndPoint; }
            set {
                mStartPoint += value - mEndPoint;
                mEndPoint = value;
            }
        }

        public Bone(Bone Parent, float Length, Quaternion2D OffsetAngle, string Name) {
            mChilds = new List<Bone>();
            mName = Name;
            mParent = Parent;
            
            if (mParent != null) {
                mParent.AddChild(this);
            }
            
            mLength = Length;
            mOffsetAngle = new Quaternion2D(OffsetAngle);
            mLocalAngle = new Quaternion2D();

            UpdateWorldAngle();
            UpdateEndPoint();
        }

        public void AddChild(Bone Child) {
            mChilds.Add(Child);
            Child.startPoint = mEndPoint;
        }

        public void UpdateChilds() {
            foreach (Bone bone in mChilds) {
                bone.mStartPoint = mEndPoint;

                bone.UpdateWorldAngle();
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

        public override string ToString() {
            return string.Format("name: {0}, angle: {2}", mName, mLocalAngle);
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

    public Bone AddBone(Bone Parent, float Length, Quaternion2D OffsetAngle, string Name) {
        Bone newBone = new Bone(Parent, Length, OffsetAngle, Name);
        mBones.Add(newBone);
        mNameToId[Name] = mBones.Count - 1;

        if (Parent == null) {
            mRoot = newBone;
        }

        return newBone;
    }

    public Bone AddBone(Bone Parent, float Length, float EualerAngle, string Name) {
        return AddBone(Parent, Length, new Quaternion2D(EualerAngle), Name);
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
            res[i] = mBones[i].localAngle.euler;
        }

        return res;
    }

    public float[] GetLocalAngles() {
        float[] res = new float[mBones.Count - 1];

        for (int i = 0; i < mBones.Count - 1; i++) {
            res[i] = mBones[i + 1].localAngle.euler;
        }

        return res;
    }

    public float[] GetLocalAnglesWithoutOffset() {
        float[] res = new float[mBones.Count - 1];

        for (int i = 0; i < mBones.Count - 1; i++) {
            res[i] = Quaternion2D.ToEuler(mBones[i + 1].localAngle / mBones[i + 1].offsetAngle);
        }

        return res;
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
