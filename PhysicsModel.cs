﻿using UnityEngine;
using System.Collections.Generic;
using System;

public class PhysicsModel {
    public class WrongCountOfAnglesException : ArgumentException {
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

        public WrongCountOfAnglesException(int ModelCount, int ArgumentCount) : base(GetMessage(ModelCount, ArgumentCount)) {
            mModelCount = ModelCount;
            mArgumentCount = ArgumentCount;
        }

        public WrongCountOfAnglesException(int ModelCount, int ArgumentCount, Exception InnerException) : base(GetMessage(ModelCount, ArgumentCount), InnerException) {
            mModelCount = ModelCount;
            mArgumentCount = ArgumentCount;
        }
    }
        
    //TODO: Дабы упростить дальнейшее добавление новых свойств, было бы неплохо заменить все это на словарь
    public struct PMObjectSettings {
        public IK.AngleLimits angleLimits;
        public GameObject prefab;
        public float mass;
        public string name;
        public int layer;
        public string tag;

        public PMObjectSettings(float lowerAngle, float UpperAngle, GameObject Prefab, float Mass, string Name, int Layer, string Tag) {
            angleLimits = new IK.AngleLimits(lowerAngle, UpperAngle);
            prefab = Prefab;
            mass = Mass;
            name = Name;
            layer = Layer;
            tag = Tag;
        }

        public PMObjectSettings(GameObject Prefab, float Mass, string Name, int Layer, string Tag) {
            angleLimits = null;
            prefab = Prefab;
            mass = Mass;
            name = Name;
            layer = Layer;
            tag = Tag;
        }
    }

    Dictionary<string, GameObject> mGameObjects;
    Dictionary<string, int> mNameToId;
    List<Manipulator> mManipulators;
    List<HingeJoint2D> mHingeJoints;

    public PhysicsModel() {
        mGameObjects = new Dictionary<string, GameObject>();
        mNameToId = new Dictionary<string, int>();
        mManipulators = new List<Manipulator>();
        mHingeJoints = new List<HingeJoint2D>();
    }
        
    public GameObject AddGameObject(GameObject ParentGameObject, Vector2 Position, Quaternion2D Angle, PMObjectSettings ObjectSettings) {
        GameObject prefabInstance = GameObject.Instantiate(ObjectSettings.prefab);
        prefabInstance.transform.RotateAround(Vector3.zero, Vector3.forward, Angle);
        prefabInstance.transform.position += new Vector3(Position.x, Position.y);
        prefabInstance.layer = ObjectSettings.layer;
        prefabInstance.tag = ObjectSettings.tag;
            
        Rigidbody2D rigidBody = prefabInstance.AddComponent<Rigidbody2D>();
        rigidBody.mass = ObjectSettings.mass;

        mGameObjects[ObjectSettings.name] = prefabInstance;

        if (ParentGameObject != null) {
            if (ObjectSettings.angleLimits.isFixed) {
                FixedJoint2D joint = prefabInstance.AddComponent<FixedJoint2D>();

                joint.connectedBody = ParentGameObject.GetComponent<Rigidbody2D>();
            } else {
                HingeJoint2D joint = prefabInstance.AddComponent<HingeJoint2D>();

                joint.autoConfigureConnectedAnchor = false;
                joint.connectedBody = ParentGameObject.GetComponent<Rigidbody2D>();
                joint.anchor = prefabInstance.transform.InverseTransformPoint(Position);
                joint.connectedAnchor = ParentGameObject.transform.InverseTransformPoint(Position);
                joint.limits = ObjectSettings.angleLimits.ToInverseJointAngleLimits2D();

                Manipulator manipulator = prefabInstance.AddComponent<Manipulator>();
                mManipulators.Add(manipulator);
                mHingeJoints.Add(joint);
                mNameToId[ObjectSettings.name] = mManipulators.Count - 1;
            }
        }

        return prefabInstance;
    }

    public Dictionary<string, int> GetAnglesOrder() {
        return new Dictionary<string, int>(mNameToId);
    }

    public Quaternion2D GetAngleById(int Id) {
        return new Quaternion2D(mManipulators[Id].angle);
    }

    public Quaternion2D GetTargetAngleById(int Id) {
        return new Quaternion2D(mManipulators[Id].targetAngle);
    }

    public void SetTargetAngle(int Id, Quaternion2D Angle) {
        mManipulators[Id].targetAngle = Angle;
    }

    public void SetTargetAngle(string Name, Quaternion2D Angle) {
        SetTargetAngle(GetIdByName(Name), Angle);
    }

    public void SetTargetAngles(Quaternion2D[] Angles) {
        if (Angles.Length != mManipulators.Count) {
            throw new WrongCountOfAnglesException(mManipulators.Count, Angles.Length);
        }

        for (int i = 0; i < Angles.Length; i++) {
            SetTargetAngle(i, Angles[i]);
        }
    }

    public Quaternion2D[] GetAngles() {
        Quaternion2D[] res = new Quaternion2D[mManipulators.Count];

        for(int i = 0; i < res.Length; i++) {
            res[i] = GetAngleById(i);
        }

        return res;
    }

    public Quaternion2D GetReferenceAngle(int Id) {
        return new Quaternion2D(mHingeJoints[Id].referenceAngle);
    }

    public Quaternion2D[] GetReferenceAngles() {
        Quaternion2D[] res = new Quaternion2D[mHingeJoints.Count];

        for (int i = 0; i < res.Length; i++) {
            res[i] = new Quaternion2D(mHingeJoints[i].referenceAngle);
        }

        return res;
    }

    public float[] GetTargetAngles() {
        float[] res = new float[mManipulators.Count];

        for (int i = 0; i < res.Length; i++) {
            res[i] = GetTargetAngleById(i);
        }

        return res;
    }

    public void SetAngleLimits(Dictionary<string, IK.AngleLimits> Limits) {
        if (Limits.Count != mHingeJoints.Count) {
            throw new WrongCountOfAnglesException(mManipulators.Count, mHingeJoints.Count);
        }

        foreach(KeyValuePair<string, IK.AngleLimits> kv in Limits) {
            mHingeJoints[mNameToId[kv.Key]].limits = kv.Value.ToJointAngleLimits2D();
        }
    }

    public int GetIdByName(string Name) {
        return mNameToId[Name];
    }

    public GameObject GetObjectByName(string Name) {
        return mGameObjects[Name];
    }

    public class SkeletonToPhysicsModelAnglesMap {
        int[] mSkToPmMap;
        int[] mPmToSkMap;
        Dictionary<string, int> mSkeletonNames;
        Dictionary<string, int> mPMNames;

        public SkeletonToPhysicsModelAnglesMap(Dictionary<string, int> SkeletonNames, Dictionary<string, int> PMNames) {
            mSkToPmMap = new int[PMNames.Count];
            mPmToSkMap = new int[SkeletonNames.Count];

            foreach (KeyValuePair<string, int> kv in PMNames) {
                mSkToPmMap[kv.Value] = SkeletonNames[kv.Key];
            }

            foreach (KeyValuePair<string, int> kv in SkeletonNames) {
                int t;
                mPmToSkMap[kv.Value] = PMNames.TryGetValue(kv.Key, out t) ? t : -1;
            }

            mSkeletonNames = SkeletonNames;
            mPMNames = PMNames;
        }

        public Quaternion2D[] ConvertSkToPmAngles(Quaternion2D[] Angles) {
            Quaternion2D[] res = new Quaternion2D[mSkToPmMap.Length];

            for (int i = 0; i < mSkToPmMap.Length; i++) {
                res[i] = Angles[mSkToPmMap[i]];
            }

            return res;
        }

        public Quaternion2D[] ConvertPmToSkAngles(Quaternion2D[] Angles) {
            Quaternion2D[] res = new Quaternion2D[mPmToSkMap.Length];

            for (int i = 0; i < mPmToSkMap.Length; i++) {
                int id = mPmToSkMap[i];
                res[i] = id >= 0 ? Angles[id] : new Quaternion2D();
            }

            return res;
        }
    }

    interface PhysicsModelCreator {
        PhysicsModel GetPhysicsModel();
    }

    public class PhysicsModelFromSkeletonCreator : PhysicsModelCreator {
        Skeleton.Bone mRoot;
        Dictionary<string, PMObjectSettings> mObjectSettings;

        public PhysicsModelFromSkeletonCreator(Skeleton.Bone Root, Dictionary<string, PMObjectSettings> ObjectSettings) {
            mRoot = Root;
            mObjectSettings = ObjectSettings;
        }

        public void CreatePhysicsModel(Skeleton.Bone Bone, GameObject Parent, PhysicsModel Model) {
            GameObject newGameObject = Model.AddGameObject(Parent, Bone.startPoint, Bone.worldAngle, mObjectSettings[Bone.name]);
            foreach (Skeleton.Bone bone in Bone.childs) {
                CreatePhysicsModel(bone, newGameObject, Model);
            }
        }

        public PhysicsModel GetPhysicsModel() {
            PhysicsModel model = new PhysicsModel();
            CreatePhysicsModel(mRoot, null, model);
            return model;
        }
    }

}
