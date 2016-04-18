using UnityEngine;
using System.Collections.Generic;
using System;

public class Replay {
    public class WrongCountOfGameObjects : Exception {
        public int Needed;
        public int Given;

        public WrongCountOfGameObjects(int aNeeded, int aGiven) :
            base(string.Format("{0} game objects is expected, but {1} is given", aNeeded, aGiven)) { }
    }

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
        public float timeDelta;

        public Frame(ObjectState[] aObjectStates, float aTimeDelta) {
            objectStates = aObjectStates;
            timeDelta = aTimeDelta;
        }
    }

    GameObject[] mGameObjects;
    Transform[] mTransforms;
    Rigidbody2D[] mRigidBodyes;
    List<Frame> mFrames;

    float mCurrentFramePlayingTime;
    int mCurrentFrameID;

    public int currentPlayFrameID { get { return mCurrentFrameID; } }
    public int frameCount { get { return mFrames.Count; } }

    void UpdateComponents() {
        for (int i = 0; i < mGameObjects.Length; i++) {
            mTransforms[i] = mGameObjects[i].GetComponent<Transform>();
            mRigidBodyes[i] = mGameObjects[i].GetComponent<Rigidbody2D>();
        }
    }

    public Replay(GameObject[] aGameObjects) {
        mGameObjects = aGameObjects;
        mTransforms = new Transform[mGameObjects.Length];
        mRigidBodyes = new Rigidbody2D[mGameObjects.Length];
        mFrames = new List<Frame>();

        UpdateComponents();
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

    public void RecordFrame(float aTimeDelta) {
        ObjectState[] objectStates = new ObjectState[mGameObjects.Length];
        for (int i = 0; i < mTransforms.Length; i++) {
            Rigidbody2D currentRigidBody = mRigidBodyes[i];
            Transform currentTransform = mTransforms[i];
            objectStates[i] = new ObjectState(currentTransform.position, currentTransform.rotation.eulerAngles.z,
                currentRigidBody.velocity, currentRigidBody.angularVelocity);
        }
        mFrames.Add(new Frame(objectStates, aTimeDelta));
    }

    void GetCurrentFrame(out ObjectState[] aCurrentObjectStates, out ObjectState[] aNextObjectStates, out float t) {
        Frame currentFrame = mFrames[mCurrentFrameID];
        Frame nextFrame = mFrames[mCurrentFrameID + 1];
        aCurrentObjectStates = currentFrame.objectStates;
        aNextObjectStates = nextFrame.objectStates;
        t = mCurrentFramePlayingTime / currentFrame.timeDelta; // between 0 and 1
    }

    public bool PlayNextFrame(float aTimeDelta) {
        mCurrentFramePlayingTime += aTimeDelta;
        while (mCurrentFrameID < mFrames.Count - 1 &&
            mFrames[mCurrentFrameID].timeDelta < mCurrentFramePlayingTime) {
            mCurrentFramePlayingTime -= mFrames[mCurrentFrameID].timeDelta;
            mCurrentFrameID++;
        }
        if (mFrames.Count == 0) {
            return false;
        } else if (mCurrentFrameID >= mFrames.Count - 1) {
            ObjectState[] objectStates = mFrames[mFrames.Count - 1].objectStates;
            for (int i = 0; i < objectStates.Length; i++) {
                mTransforms[i].position = objectStates[i].position;
                mTransforms[i].rotation = Quaternion.Euler(0, 0, objectStates[i].rotation);
            }
            return false;
        } else {
            ObjectState[] currentObjectStates, nextObjectStates;
            float t;
            GetCurrentFrame(out currentObjectStates, out nextObjectStates, out t);
            for (int i = 0; i < mTransforms.Length; i++) {
                mTransforms[i].position = Vector2.Lerp(currentObjectStates[i].position, nextObjectStates[i].position, t);
                mTransforms[i].rotation = Quaternion.Euler(0, 0, LerpAngle(currentObjectStates[i].rotation, nextObjectStates[i].rotation, t));
            }
            return true;
        }
    }

    public void PrepareToSimulation() {
        if (mFrames.Count == 0) {
            SetObjectsIsKinematic(false);
        } else if (mCurrentFrameID >= mFrames.Count - 1) {
            ObjectState[] currentObjectStates = mFrames[mFrames.Count - 1].objectStates;
            for (int i = 0; i < mRigidBodyes.Length; i++) {
                mRigidBodyes[i].isKinematic = false;

                mRigidBodyes[i].velocity = currentObjectStates[i].linearVelocity;
                mRigidBodyes[i].angularVelocity = currentObjectStates[i].angularVelocity;
            }
        } else {
            ObjectState[] currentObjectStates, nextObjectStates;
            float t;
            GetCurrentFrame(out currentObjectStates, out nextObjectStates, out t);
            for (int i = 0; i < mRigidBodyes.Length; i++) {
                mRigidBodyes[i].isKinematic = false;

                mRigidBodyes[i].velocity = Vector2.Lerp(currentObjectStates[i].linearVelocity, nextObjectStates[i].linearVelocity, t);
                mRigidBodyes[i].angularVelocity = LerpAngle(currentObjectStates[i].angularVelocity, nextObjectStates[i].angularVelocity, t);
            }
        }
    }
    
    public void ReplaceGameObjects(Dictionary<GameObject, GameObject> aRule) {
        for(int i = 0; i < mGameObjects.Length; i++) {
            GameObject newGameObject;
            if (aRule.TryGetValue(mGameObjects[i], out newGameObject)) {
                mGameObjects[i] = newGameObject;
            }
        }
        UpdateComponents();
    }

    public void SetObjectsIsKinematic(bool aIsKinematic) {
        for (int i = 0; i < mRigidBodyes.Length; i++) {
            mRigidBodyes[i].isKinematic = aIsKinematic;
        }
    }

    public void EraseRecorded() {
        mFrames.Clear();
    }

    public void EraseAfterwardsFrames() {
        if (mCurrentFrameID < mFrames.Count - 1) {
            mFrames.RemoveRange(mCurrentFrameID + 1, mFrames.Count - mCurrentFrameID - 1);
        }
    }

    public void ConcatenateReplay(Replay aReplay) {
        if (mGameObjects.Length != aReplay.mGameObjects.Length) {
            throw new WrongCountOfGameObjects(mGameObjects.Length, aReplay.mGameObjects.Length);
        }
        mFrames.AddRange(aReplay.mFrames);
    }

    public void ToStart() {
        mCurrentFrameID = 0;
        mCurrentFramePlayingTime = 0;
    }
}
