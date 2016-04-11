using UnityEngine;
using System.Collections;

public class TargetFollower : MonoBehaviour {
    public float zDistance;
    public Transform Target;
    public float Speed;

    Transform mFollower;

    void Awake() {
        mFollower = GetComponent<Transform>();
        Speed = 1;
    }
	
	void Update () {
        Vector3 followerPos = mFollower.position;
        Vector3 targetPos = Target.position;

        mFollower.position = new Vector3(0, 0, zDistance) + followerPos + (targetPos - followerPos) * Speed;
	}
}
