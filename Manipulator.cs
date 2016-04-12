using UnityEngine;
using System.Collections;

public class Manipulator : MonoBehaviour {
    Quaternion2D mTargetAngle;
    public float multiplier;

    public float force { get { return mJoint.motor.maxMotorTorque; } }

    public Quaternion2D targetAngle {
        get { return mTargetAngle.Clone(); }
        set { mTargetAngle = value.Clone(); }
    }

    public Quaternion2D angle { get { return new Quaternion2D(-mJoint.jointAngle); } }

    public Quaternion2D referenceAngle {
        get {
            return new Quaternion2D(mJoint.referenceAngle);
        }
        set {
            Rigidbody2D rigidBody = mJoint.connectedBody;
            Quaternion rotation = mTransform.rotation;
            Vector3 position = mTransform.position;
            float parentAngle = rigidBody.gameObject.GetComponent<Transform>().rotation.eulerAngles.z;
            mTransform.rotation = new Quaternion();
            mTransform.RotateAround(mTransform.TransformPoint(mJoint.anchor), Vector3.forward, parentAngle - value);
            mJoint.connectedBody = rigidBody;
            mTransform.position = position;
            mTransform.rotation = rotation;
        }
    }

    HingeJoint2D mJoint;
    Rigidbody2D mRigidBody;
    Transform mTransform;

    public void Awake() {
        mJoint = GetComponent<HingeJoint2D>();
        mTargetAngle = new Quaternion2D(mJoint.jointAngle);
        mRigidBody = GetComponent<Rigidbody2D>();
        mTransform = GetComponent<Transform>();
    }

    void FixedUpdate() {
        float Err = -targetAngle.euler - mJoint.jointAngle;
        JointMotor2D motor = new JointMotor2D();
        motor.maxMotorTorque = 0.03f;
        motor.motorSpeed = Err * Err * Err * Time.fixedDeltaTime / 10;
        //motor.motorSpeed = 50 * Err * Time.fixedDeltaTime;
        mJoint.motor = motor;
    }
}
