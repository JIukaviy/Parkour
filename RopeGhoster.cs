using UnityEngine;
using System.Collections;
using System;

public class RopeGhoster : MonoBehaviour, GhostCreator.Ghoster {
    GameObject[] mChainElements;

    void Start() {
        mChainElements = GetComponent<RopeCreator>().GetChainElements();
        GhostCreator.RegisterGhoster(this);
    }

    public void CreateGhost() {
        GameObject parent = null;

        for (int i = 1; i < mChainElements.Length; i++) {
            GameObject ghost = Instantiate(mChainElements[i]);
            if (parent != null) {
                ghost.GetComponent<HingeJoint2D>().connectedBody = parent.GetComponent<Rigidbody2D>();
            }
            GhostCreator.RegisterGhost(ghost, mChainElements[i]);
            parent = ghost;
        }
    }
}
