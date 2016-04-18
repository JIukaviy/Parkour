using UnityEngine;
using System.Collections;
using System;

public class Timer : MonoBehaviour {
    float mTimeCounter;
    float mInterval;

    public EventHandler OnElapsed;
    public float interval {
        get { return mInterval; }
        set { mInterval = value; }
    }

    public void StartTimer() {
        enabled = true;
        mTimeCounter = 0;
    }

    public void StopTimer() {
        enabled = false;
        mTimeCounter = 0;
    }

    void Awake() {
        enabled = false;
    }
	
	void Update() {
        mTimeCounter += Time.deltaTime;
        if (mTimeCounter >= mInterval) {
            mTimeCounter = 0;
            if (OnElapsed != null) {
                OnElapsed.Invoke(this, new EventArgs());
                Debug.Log("GHOSTS IS DEAD!");
            }
        }
	}
}
