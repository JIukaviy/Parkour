using UnityEngine;
using System.Collections;

public class HandGrabber : MonoBehaviour {
    public enum GrabType {
        FixedGrab,
        HingeGrab
    }

    public GrabType grabType = GrabType.HingeGrab;
    public bool grabbed { get { return mGrabbed; } }
    public bool canGrab {
        get { return mCanGrab; }
        set {
            if (mGrabbed && !value) {
                Ungrab();
            }
            mCanGrab = value;
        }
    }

    bool mGrabbed;
    bool mCanGrab;
    
    Rigidbody2D mCollidedBody;
    Vector2 mCollissionPoint;
    Joint2D mJoint;

    void OnCollisionEnter2D(Collision2D coll) {
        if (!grabbed && mCanGrab) {
            if (grabType == GrabType.HingeGrab) {
                mJoint = HingeGrab(coll.gameObject, coll.contacts[0].point);
            } else if (grabType == GrabType.FixedGrab) {
                mJoint = FixedGrab(coll.gameObject);
            }
            mCollidedBody = coll.rigidbody;
            mCollissionPoint = coll.contacts[0].point;
            mGrabbed = true;
        }
    }

    Joint2D HingeGrab(GameObject aGameObject, Vector2 aPoint) {
        HingeJoint2D joint = gameObject.AddComponent<HingeJoint2D>();
        Rigidbody2D collidedRigidBody = aGameObject.GetComponent<Rigidbody2D>();
        Transform collidedTransform = aGameObject.GetComponent<Transform>();

        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = transform.InverseTransformPoint(aPoint);
        joint.connectedBody = collidedRigidBody;
        joint.connectedAnchor = collidedTransform.InverseTransformPoint(aPoint);

        return joint;
    }

    Joint2D FixedGrab(GameObject aGameObject) {
        FixedJoint2D joint = gameObject.AddComponent<FixedJoint2D>();
        joint.connectedBody = aGameObject.GetComponent<Rigidbody2D>();

        return joint;
    }

    public void ToggleGrabType() {
        if (grabType == GrabType.FixedGrab) {
            grabType = GrabType.HingeGrab;
        } else {
            grabType = GrabType.FixedGrab;
        }
    }

    public void ToggleGrab() {
        canGrab = !canGrab;
    }

    public void Ungrab() {
        if (mGrabbed) {
            Destroy(mJoint);
            mJoint = null;
        }
        mGrabbed = false;
    }

    public void OnCopy(HandGrabber aOriginal) {
        mJoint = aOriginal.mJoint;
        mGrabbed = aOriginal.mGrabbed;
        grabType = aOriginal.grabType;
        mCollidedBody = aOriginal.mCollidedBody;
        mCollissionPoint = aOriginal.mCollissionPoint;
        canGrab = aOriginal.canGrab;
    }
}
