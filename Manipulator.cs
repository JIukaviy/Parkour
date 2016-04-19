using UnityEngine;
using System.Collections;

public class Manipulator : MonoBehaviour {
    float mTargetAngle;
    public float multiplier;

    public float force { get { return mJoint.motor.maxMotorTorque; } }

    public float targetAngle {
        get { return mTargetAngle; }
        set { mTargetAngle = value; }
    }

    public float angle { get { return -mJoint.jointAngle; } }

    public float referenceAngle { get { return mJoint.referenceAngle; } }

    public void SetReferenceAngle(float refAngle, float jointAngle) {
        jointAngle = -jointAngle;
        Rigidbody2D parentRigidBody = mJoint.connectedBody;
        float rotation = mRigidBody.rotation;
        mRigidBody.rotation = parentRigidBody.rotation - refAngle;
        mJoint.connectedBody = parentRigidBody;
        mRigidBody.rotation = rotation;
        while (mJoint.jointAngle < jointAngle - 0.0001f)
            mRigidBody.rotation -= 360f;
        while (mJoint.jointAngle > jointAngle + 0.0001f)
            mRigidBody.rotation += 360f;
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

    public void OnCopy(Manipulator aOriginal) {
        SetReferenceAngle(aOriginal.referenceAngle, aOriginal.angle);
        targetAngle = aOriginal.targetAngle;
    }

    void FixedUpdate() {
        float Err = -targetAngle - mJoint.jointAngle;
        JointMotor2D motor = new JointMotor2D();
        motor.maxMotorTorque = 0.03f;
        motor.motorSpeed = Err * Err * Err * Time.fixedDeltaTime / 10;
        //motor.motorSpeed = 50 * Err * Time.fixedDeltaTime;
        mJoint.motor = motor;
    }
}
