using UnityEngine;
using System.Collections.Generic;
using System;

public static class GhostCreator {
    public interface Ghoster {
        void CreateGhost();
    }

    class GhostInfo {
        GameObject mGhost;
        GameObject mOriginal;

        Vector2 mStartPosition;
        float mStartOriginalRotation;
        float mStartGhostRotation;

        Vector2 mStartLinearVelocity;
        float mStartAngularVelocity;

        Transform mOriginalTransform;
        Rigidbody2D mOriginalRigidBody;

        Transform mGhostTransform;
        Rigidbody2D mGhostRigidBody;

        Action mOnRestoreGhost;

        public GameObject originalGameObject { get { return mOriginal; } }
        public GameObject ghostGameObject { get { return mGhost; } }

        public GhostInfo(GameObject Ghost, GameObject Original, Action OnRestoreGhost = null) {
            mGhost = Ghost;
            mOriginal = Original;

            mOriginalTransform = Original.GetComponent<Transform>();
            mOriginalRigidBody = Original.GetComponent<Rigidbody2D>();

            mGhostTransform = Ghost.GetComponent<Transform>();
            mGhostRigidBody = Ghost.GetComponent<Rigidbody2D>();

            mStartPosition = mOriginalRigidBody.position;
            mStartOriginalRotation = mOriginalRigidBody.rotation;
            mStartGhostRotation = mGhostRigidBody.rotation;

            mStartLinearVelocity = mOriginalRigidBody.velocity;
            mStartAngularVelocity = mOriginalRigidBody.angularVelocity;

            mOriginalRigidBody.isKinematic = true;

            mOnRestoreGhost = OnRestoreGhost;

            mGhostRigidBody.angularVelocity = mStartAngularVelocity;
            mGhostRigidBody.velocity = mStartLinearVelocity;
        }

        public void DestroyGhost() {
            GameObject.Destroy(mGhost);
        }

        public void RestoreGhostState() {
            mGhostRigidBody.isKinematic = false;

            if (mOnRestoreGhost != null) {
                mOnRestoreGhost();
            }

            mGhostRigidBody.position = mStartPosition;
            mGhostRigidBody.rotation = mStartGhostRotation;

            mGhostRigidBody.velocity = mStartLinearVelocity;
            mGhostRigidBody.angularVelocity = mStartAngularVelocity;
        }

        public void RestoreOriginalObjectState() {
            mOriginalRigidBody.isKinematic = false;

            mOriginalRigidBody.velocity = mStartLinearVelocity;
            mOriginalRigidBody.angularVelocity = mStartAngularVelocity;
        }
    }

    static List<Ghoster> mGhosters = new List<Ghoster>();
    static List<GhostInfo> mGhostsInfo = new List<GhostInfo>();
    static Dictionary<int, int> mLayerConverter = new Dictionary<int, int>();
    static Dictionary<GameObject, GameObject> mGhostsToOriginal = new Dictionary<GameObject, GameObject>();
    static Timer mGhostLivingTimer;

    static GhostCreator() {
        GameObject timerObject = new GameObject();
        mGhostLivingTimer = timerObject.AddComponent<Timer>();
        mGhostLivingTimer.OnElapsed += OnTimer;
    }

    static public Dictionary<GameObject, GameObject> ghostToOriginal {
        get { return mGhostsToOriginal; }
    }

    static public EventHandler OnGhostLivingTimeExceeded;

    static public void RegisterLayerConverter(int aFrom, int aTo) {
        mLayerConverter[aFrom] = aTo;
    }

    static public void RegisterGhoster(Ghoster aGhoster) {
        mGhosters.Add(aGhoster);
    }

    static public void RegisterGhost(GameObject Ghost, GameObject Original, Action OnRestoreObject = null) {
        mGhostsInfo.Add(new GhostInfo(Ghost, Original, OnRestoreObject));
        int newLayer;
        if (mLayerConverter.TryGetValue(Ghost.layer, out newLayer)) {
            Ghost.layer = newLayer;
        }
        mGhostsToOriginal[Ghost] = Original;
    }
	
	static public void CreateGhosts(float aLivingTime = 0) {
        foreach (Ghoster g in mGhosters) {
            g.CreateGhost();
        }
        SetUpTimer(aLivingTime);
    }

    static public void DeleteGhosts() {
        foreach(GhostInfo ghostInfo in mGhostsInfo) {
            ghostInfo.DestroyGhost();
            ghostInfo.RestoreOriginalObjectState();
        }
        mGhostsInfo.Clear();
        mGhostsToOriginal.Clear();
    }

    static public void RestoreGhostState(float aLivingTime = 0) {
        foreach (GhostInfo ghostInfo in mGhostsInfo) {
            ghostInfo.RestoreGhostState();
        }
        SetUpTimer(aLivingTime);
    }

    static public GameObject[] GetGhostsGameObjects() {
        GameObject[] res = new GameObject[mGhostsInfo.Count];
        for (int i = 0; i < mGhostsInfo.Count; i++) {
            res[i] = mGhostsInfo[i].ghostGameObject;
        }
        return res;
    }

    static void SetUpTimer(float aLivingTime) {
        if (aLivingTime > 0) {
            mGhostLivingTimer.interval = aLivingTime;
            mGhostLivingTimer.StartTimer();
        } else {
            mGhostLivingTimer.StopTimer();
        }
    }

    static void OnTimer(object sender, EventArgs args) {
        mGhostLivingTimer.enabled = false;
        if (OnGhostLivingTimeExceeded != null) {
            OnGhostLivingTimeExceeded.Invoke(sender, args);
        }
    }
}
