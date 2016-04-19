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

        public Action mOnCreationEnd;

        public GhostInfo(GameObject aGhost, GameObject aOriginal, Action aOnRestoreGhost = null, Action aOnCreationEnd = null) {
            mGhost = aGhost;
            mOriginal = aOriginal;

            mOriginalTransform = aOriginal.GetComponent<Transform>();
            mOriginalRigidBody = aOriginal.GetComponent<Rigidbody2D>();

            mGhostTransform = aGhost.GetComponent<Transform>();
            mGhostRigidBody = aGhost.GetComponent<Rigidbody2D>();

            mStartPosition = mOriginalRigidBody.position;
            mStartOriginalRotation = mOriginalRigidBody.rotation;
            mStartGhostRotation = mGhostRigidBody.rotation;

            mStartLinearVelocity = mOriginalRigidBody.velocity;
            mStartAngularVelocity = mOriginalRigidBody.angularVelocity;

            mOriginalRigidBody.isKinematic = true;

            mOnRestoreGhost = aOnRestoreGhost;
            mOnCreationEnd = aOnCreationEnd;

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
    static Dictionary<GameObject, GameObject> mOriginalToGhosts = new Dictionary<GameObject, GameObject>();
    static Timer mGhostLivingTimer;

    static GhostCreator() {
        GameObject timerObject = new GameObject();
        mGhostLivingTimer = timerObject.AddComponent<Timer>();
        mGhostLivingTimer.OnElapsed += OnTimer;
    }

    static public Dictionary<GameObject, GameObject> ghostToOriginal {
        get { return mGhostsToOriginal; }
    }

    static public Dictionary<GameObject, GameObject> originalToGhost {
        get { return mOriginalToGhosts; }
    }

    static public EventHandler OnGhostLivingTimeExceeded;

    static public void RegisterLayerConverter(int aFrom, int aTo) {
        mLayerConverter[aFrom] = aTo;
    }

    static public void RegisterGhoster(Ghoster aGhoster) {
        mGhosters.Add(aGhoster);
    }

    static public void RegisterGhost(GameObject aGhost, GameObject aOriginal, Action aOnRestoreObject = null, Action aOnGhostsCreationEnd = null) {
        mGhostsInfo.Add(new GhostInfo(aGhost, aOriginal, aOnRestoreObject, aOnGhostsCreationEnd));
        int newLayer;
        if (mLayerConverter.TryGetValue(aGhost.layer, out newLayer)) {
            aGhost.layer = newLayer;
        }
        mGhostsToOriginal[aGhost] = aOriginal;
        mOriginalToGhosts[aOriginal] = aGhost;
    }
	
	static public void CreateGhosts(float aLivingTime = 0) {
        foreach (Ghoster g in mGhosters) {
            g.CreateGhost();
        }
        for (int i = 0; i < mGhostsInfo.Count; i++) {
            if (mGhostsInfo[i].mOnCreationEnd != null) {
                mGhostsInfo[i].mOnCreationEnd();
            }
        }
        SetUpTimer(aLivingTime);
    }

    static public void DeleteGhosts() {
        for (int i = 0; i < mGhostsInfo.Count; i++) {
            mGhostsInfo[i].DestroyGhost();
            mGhostsInfo[i].RestoreOriginalObjectState();
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

    static public GameObject GetGhostByOriginal(GameObject Original) {
        GameObject res = null;
        mOriginalToGhosts.TryGetValue(Original, out res);
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
