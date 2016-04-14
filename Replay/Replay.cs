using UnityEngine;
using System.Collections.Generic;
using System;

public class Replay {
    struct ObjectState {
        public Vector2 position;
        public float rotation;
        public Vector2 linearVelocity;
        public float angularVelocity;

        public ObjectState(Vector2 aPosition, float aRotation, Vector2 aLinearVelocity, float aAngularVelocity) {
            position = aPosition;
            rotation = aRotation;
            linearVelocity = aLinearVelocity;
            angularVelocity = aAngularVelocity;
        }
    }

    struct Frame {
        public ObjectState[] objectStates;
        public float time;

        public Frame(ObjectState[] aObjectStates, float Time) {
            objectStates = aObjectStates;
            time = Time;
        }
    }

    GameObject[] mGameObjects;
    Transform[] mTransforms;
    Rigidbody2D[] mRigidBodyes;
    List<Frame> mFrames;

    float mCurrentRecordTime;
    float mCurrentPlayTime;
    int mCurrentFrameID;

    public Replay(GameObject[] aGameObjects) {
        mGameObjects = aGameObjects;
        mTransforms = new Transform[mGameObjects.Length];
        mRigidBodyes = new Rigidbody2D[mGameObjects.Length];
        mFrames = new List<Frame>();

        for (int i = 0; i < mGameObjects.Length; i++) {
            mTransforms[i] = mGameObjects[i].GetComponent<Transform>();
            mRigidBodyes[i] = mGameObjects[i].GetComponent<Rigidbody2D>();
        }
    }

    float LerpAngle(float a1, float a2, float t) {
        if (a1 - a2 > 180) {
            a1 -= 360;
        }
        if (a1 - a2 < -180) {
            a1 += 360;
        }
        return a1 + t * (a2 - a1);
    }

    public void RecordFrame(float aTimeStep) {
        ObjectState[] objectStates = new ObjectState[mGameObjects.Length];
        for (int i = 0; i < mTransforms.Length; i++) {
            Rigidbody2D currentRigidBody = mRigidBodyes[i];
            Transform currentTransform = mTransforms[i];
            objectStates[i] = new ObjectState(currentTransform.position, currentTransform.rotation.eulerAngles.z,
                currentRigidBody.velocity, currentRigidBody.angularVelocity);
        }
        mFrames.Add(new Frame(objectStates, mCurrentRecordTime));
        mCurrentRecordTime += aTimeStep;
    }
    
    public bool PlayNextFrame(float aTimeStep) {
        while (mCurrentFrameID < mFrames.Count - 1 && 
            mFrames[mCurrentFrameID].time < mCurrentPlayTime && mFrames[mCurrentFrameID + 1].time < mCurrentPlayTime) 
        {
            mCurrentFrameID++;
        }
        if (mCurrentFrameID >= mFrames.Count - 1) {
            ObjectState[] objectStates = mFrames[mFrames.Count - 1].objectStates;
            for (int i = 0; i < objectStates.Length; i++) {
                mTransforms[i].position = objectStates[i].position;
                mTransforms[i].rotation = Quaternion.Euler(0, 0, objectStates[i].rotation);
            }
            return false;
        } else {
            Frame currentFrame = mFrames[mCurrentFrameID];
            Frame nextFrame = mFrames[mCurrentFrameID + 1];
            ObjectState[] currentObjectStates = currentFrame.objectStates;
            ObjectState[] nextObjectStates = nextFrame.objectStates;
            float t = (mCurrentPlayTime - currentFrame.time) / (nextFrame.time - currentFrame.time); // between 0 and 1
            for (int i = 0; i < mTransforms.Length; i++) {
                mTransforms[i].position = Vector2.Lerp(currentObjectStates[i].position, nextObjectStates[i].position, t);
                mTransforms[i].rotation = Quaternion.Euler(0, 0, LerpAngle(currentObjectStates[i].rotation, nextObjectStates[i].rotation, t));
            }
            mCurrentPlayTime += aTimeStep;
            return true;
        }
    }

    public void SetObjectsIsKinematic(bool aIsKinematic) {
        for (int i = 0; i < mRigidBodyes.Length; i++) {
            mRigidBodyes[i].isKinematic = aIsKinematic;
        }
    }

    public void ToStart() {
        mCurrentFrameID = 0;
        mCurrentPlayTime = 0;
    }
}
