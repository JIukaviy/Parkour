using UnityEngine;
using System.Collections;

public class Manipulator : MonoBehaviour {
    public float targetAngle;
    public float multiplier;
    public float force {
        get {
            return mJoint.motor.maxMotorTorque;
        }
        set {
            JointMotor2D motor = mJoint.motor;
            motor.maxMotorTorque = value;
            mJoint.motor = motor;
        }
    }
    public float angle {
        get {
            return mJoint.jointAngle;
        }
    }

    private HingeJoint2D mJoint;

    public void Start() {
        mJoint = GetComponent<HingeJoint2D>();
        targetAngle = mJoint.jointAngle;
        multiplier = 1.0f;
    }

    void Update() {
        JointMotor2D motor = mJoint.motor;
        motor.motorSpeed = (targetAngle - mJoint.jointAngle) * multiplier;
        mJoint.motor = motor;
    }
}
