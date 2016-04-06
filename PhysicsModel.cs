using UnityEngine;
using System.Collections.Generic;
using System;

namespace PhysicsModel {
    public class PhysicsModel {
        public class WrondCountOfAnglesException : ArgumentException {
            int mModelCount;
            int mArgumentCount;

            public int modelCount {
                get {
                    return mModelCount;
                }
            }

            public int ArgumentCount {
                get {
                    return mArgumentCount;
                }
            }

            static string GetMessage(int ModelCount, int ArgumentCount) {
                return String.Format("Expected array[{0}] but array[{1}] is given", ModelCount, ArgumentCount);
            }

            public WrondCountOfAnglesException(int ModelCount, int ArgumentCount) : base(GetMessage(ModelCount, ArgumentCount)) {
                mModelCount = ModelCount;
                mArgumentCount = ArgumentCount;
            }

            public WrondCountOfAnglesException(int ModelCount, int ArgumentCount, Exception InnerException) : base(GetMessage(ModelCount, ArgumentCount), InnerException) {
                mModelCount = ModelCount;
                mArgumentCount = ArgumentCount;
            }
        }

        List<GameObject> mGameObjects;
        Dictionary<string, int> mNameToId;
        List<Manipulator> mManipulators;
        List<HingeJoint2D> mHingJoints;
        int mLayer;

        public PhysicsModel(int Layer) {
            mGameObjects = new List<GameObject>();
            mNameToId = new Dictionary<string, int>();
            mManipulators = new List<Manipulator>();
            mHingJoints = new List<HingeJoint2D>();
            mLayer = Layer;
        }
        
        public GameObject AddGameObject(Vector2 Position, float Angle, IK.AngleLimits AngleLimits, GameObject ParentGameObject, GameObject Prefab, string Name) {
            GameObject prefabInstance = GameObject.Instantiate(Prefab);
            prefabInstance.transform.RotateAround(Vector3.zero, Vector3.forward, Angle);
            prefabInstance.transform.position += new Vector3(Position.x, Position.y);
            prefabInstance.layer = mLayer;
            prefabInstance.AddComponent<Rigidbody2D>();
            prefabInstance.GetComponent<Rigidbody2D>().useAutoMass = true;

            if (ParentGameObject != null) {
                prefabInstance.AddComponent<HingeJoint2D>();
                HingeJoint2D joint = prefabInstance.GetComponent<HingeJoint2D>();

                joint.autoConfigureConnectedAnchor = false;
                joint.limits = AngleLimits.ToJointAngleLimits2D();
                joint.connectedBody = ParentGameObject.GetComponent<Rigidbody2D>();
                joint.anchor = prefabInstance.transform.InverseTransformPoint(Position);
                joint.connectedAnchor = ParentGameObject.transform.InverseTransformPoint(Position);

                prefabInstance.AddComponent<Manipulator>();
                Manipulator manipulator = prefabInstance.GetComponent<Manipulator>();
                manipulator.Start();
                manipulator.multiplier = 10;
                mManipulators.Add(manipulator);
                mHingJoints.Add(joint);
                mNameToId[Name] = mManipulators.Count - 1;
            }

            return prefabInstance;
        }

        public Dictionary<string, int> GetAnglesOrder() {
            return new Dictionary<string, int>(mNameToId);
        }

        public float GetAngleById(int Id) {
            return mManipulators[Id].angle;
        }

        public void SetTargetAngle(int Id, float Angle) {
            mManipulators[Id].targetAngle = Angle;
        }

        public void SetTargetAngle(string Name, float Angle) {
            mManipulators[GetIdByName(Name)].targetAngle = Angle;
        }

        public void SetTargetAngles(float[] Angles) {
            if (Angles.Length != mManipulators.Count) {
                throw new WrondCountOfAnglesException(mManipulators.Count, Angles.Length);
            }

            for (int i = 0; i < Angles.Length; i++) {
                SetTargetAngle(i, Angles[i]);
            }
        }

        public float[] GetTargetAngles() {
            float[] res = new float[mManipulators.Count];

            for(int i = 0; i < res.Length; i++) {
                res[i] = mManipulators[i].angle;
            }

            return res;
        }

        public void SetAngleLimits(Dictionary<string, IK.AngleLimits> Limits) {
            if (Limits.Count != mHingJoints.Count) {
                throw new WrondCountOfAnglesException(mManipulators.Count, mHingJoints.Count);
            }

            foreach(KeyValuePair<string, IK.AngleLimits> kv in Limits) {
                mHingJoints[mNameToId[kv.Key]].limits = kv.Value.ToJointAngleLimits2D();
            }
        }

        public int GetIdByName(string Name) {
            return mNameToId[Name];
        }
    }
    
    interface PhysicsModelCreator {
        PhysicsModel GetPhysicsModel();
    }

    public class PhysicsModelFromSkeletonCreator : PhysicsModelCreator {
        Skeleton.Bone mRoot;
        Dictionary<string, IK.AngleLimits> mAngleLimits;
        Dictionary<string, GameObject> mPrefabs;
        int mLayer;

        public PhysicsModelFromSkeletonCreator(Skeleton.Bone Root, Dictionary<string, IK.AngleLimits> AngleLimits, Dictionary<string, GameObject> Prefabs, int Layer) {
            mRoot = Root;
            mAngleLimits = AngleLimits;
            mPrefabs = Prefabs;
            mLayer = Layer;
        }

        public void GetPhysicsModel(Skeleton.Bone Bone, GameObject Parent, PhysicsModel Model) {
            GameObject newGameObject = Model.AddGameObject(Bone.startPoint, Bone.angle, mAngleLimits[Bone.name], Parent, mPrefabs[Bone.name], Bone.name);
            foreach(Skeleton.Bone bone in Bone.childs) {
                GetPhysicsModel(bone, newGameObject, Model);
            }
        }

        public PhysicsModel GetPhysicsModel() {
            PhysicsModel model = new PhysicsModel(mLayer);
            GetPhysicsModel(mRoot, null, model);
            return model;
        }
    }

    public class SkeletonToPhysicsModelAnglesMap {
        int[] mMap;
        float[] mOffsets;
        Dictionary<string, int> mSkeletonNames;
        Dictionary<string, int> mPMNames;

        public SkeletonToPhysicsModelAnglesMap(Dictionary<string, int> SkeletonNames, Dictionary<string, int> PMNames, float[] SkeletonAngles, float[] PMAngles) {
            if (SkeletonNames.Count != PMNames.Count) {
                throw new PhysicsModel.WrondCountOfAnglesException(PMNames.Count, SkeletonNames.Count);
            }

            if (SkeletonAngles.Length != PMAngles.Length) {
                throw new PhysicsModel.WrondCountOfAnglesException(PMNames.Count, SkeletonNames.Count);
            }

            mMap = new int[SkeletonNames.Count];

            foreach(KeyValuePair<string, int> kv in PMNames) {
                mMap[SkeletonNames[kv.Key]] = kv.Value;
            }

            mOffsets = new float[SkeletonAngles.Length];

            for (int i = 0; i < mOffsets.Length; i++) {
                mOffsets[i] = PMAngles[i] - SkeletonAngles[mMap[i]];
            }

            mSkeletonNames = SkeletonNames;
            mPMNames = PMNames;
        }

        public float[] ConvertAngles(float[] Angles) {
            if (Angles.Length != mMap.Length) {
                throw new PhysicsModel.WrondCountOfAnglesException(mMap.Length, Angles.Length);
            }

            float[] res = new float[mMap.Length];

            for (int i = 0; i < mMap.Length; i++) {
                res[i] = -Angles[mMap[i]] - mOffsets[i];
            }

            return res;
        }

        public Dictionary<string, IK.AngleLimits> ConvertAngleLimits(Dictionary<string, IK.AngleLimits> Limits) {
            Dictionary<string, IK.AngleLimits> res = new Dictionary<string, IK.AngleLimits>();

            foreach(KeyValuePair<string, int> kv in mPMNames) {
                res[kv.Key] = new IK.AngleLimits(-Limits[kv.Key].maxAngle - mOffsets[kv.Value], -Limits[kv.Key].minAngle - mOffsets[kv.Value]);
            }

            return res;
        }
    }
}
