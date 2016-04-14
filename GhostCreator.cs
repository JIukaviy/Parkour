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
            mGhostRigidBody.position = mStartPosition;
            mGhostRigidBody.rotation = mStartGhostRotation;

            mGhostRigidBody.velocity = mStartLinearVelocity;
            mGhostRigidBody.angularVelocity = mStartAngularVelocity;

            if (mOnRestoreGhost != null) {
                mOnRestoreGhost();
            }
        }

        public void RestoreOriginalObjectState() {
            mOriginalRigidBody.isKinematic = false;

            mOriginalRigidBody.velocity = mStartLinearVelocity;
            mOriginalRigidBody.angularVelocity = mStartAngularVelocity;
        }
    }
    
    static List<Ghoster> mGhosters = new List<Ghoster>();
    static List<GhostInfo> mGhostsInfo = new List<GhostInfo>();

    static public void RegisterGhoster(Ghoster aGhoster) {
        mGhosters.Add(aGhoster);
    }

    static public void RegisterGhost(GameObject Ghost, GameObject Original, Action OnRestoreObject = null) {
        mGhostsInfo.Add(new GhostInfo(Ghost, Original, OnRestoreObject));
    }
	
	static public void CreateGhosts() {
        foreach (Ghoster g in mGhosters) {
            g.CreateGhost();
        }
    }

    static public void DeleteGhosts() {
        foreach(GhostInfo ghostInfo in mGhostsInfo) {
            ghostInfo.DestroyGhost();
            ghostInfo.RestoreOriginalObjectState();
        }
        mGhostsInfo.Clear();
    }

    static public void RestoreGhostState() {
        foreach (GhostInfo ghostInfo in mGhostsInfo) {
            ghostInfo.RestoreGhostState();
        }
    }
}
