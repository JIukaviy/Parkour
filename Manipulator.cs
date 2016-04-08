using UnityEngine;
using System.Collections;

public class Manipulator : MonoBehaviour {
    Quaternion2D mTargetAngle;
    public float multiplier;
    public float maxSpeed;
    public float P;
    public float I;
    public float D;

    public float force {
        get { return mJoint.motor.maxMotorTorque; }
        set {
            JointMotor2D motor = mJoint.motor;
            motor.maxMotorTorque = value;
            mJoint.motor = motor;
        }
    }

    public Quaternion2D targetAngle {
        get { return mTargetAngle.Clone(); }
        set { mTargetAngle = value.Clone(); }
    }

    public Quaternion2D angle { get { return new Quaternion2D(-mJoint.jointAngle); } }

    HingeJoint2D mJoint;


    public void Start() {
        mJoint = GetComponent<HingeJoint2D>();
        mTargetAngle = new Quaternion2D(mJoint.jointAngle);
        multiplier = 0.01f;
        maxSpeed = 20;
        force = 200;
        
        intErr = 0;
        prevErr = 0;

        P = 1f;
        I = 0f;
        D = 3;
    }
    
    float prevErr;
    float intErr;

    void Update() {
        JointMotor2D motor = mJoint.motor;
        float Err = targetAngle / angle;
        float dErr = Err - prevErr;
        float U = P * Err + I * intErr + D * dErr * Time.deltaTime;
        motor.motorSpeed = -Err * 2 - D * dErr * Time.deltaTime;
        prevErr = Err;
        intErr += Err / Time.deltaTime;
        mJoint.motor = motor;
    }
}
