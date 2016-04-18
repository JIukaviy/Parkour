using UnityEngine;
using System.Collections;
using System;

public class ReplayPlayer : MonoBehaviour {
    public EventHandler OnReplayEnd;

    Replay mReplay;

    public bool playing { get { return enabled; } }
    public Replay replay { get { return mReplay; } }

    void Awake() {
        enabled = false;
    }

    public void Play(Replay aReplay) {
        mReplay = aReplay;
        mReplay.SetObjectsIsKinematic(true);
        enabled = true;
    }

    public void Pause() {
        enabled = false;
    }

    public void Continue() {
        if (mReplay != null) {
            enabled = true;
        }
    }

    public void Stop() {
        enabled = false;
        if (mReplay != null) {
            mReplay.SetObjectsIsKinematic(false);
            mReplay = null;
        }
    }

    public void ToStart() {
        if (mReplay != null) {
            mReplay.ToStart();
            enabled = false;
        }
    }

    public void EraseAfterwardsFrames() {
        if (mReplay != null) {
            mReplay.EraseAfterwardsFrames();
            enabled = false;
        }
    }

    public void PrepareToSumulation() {
        if (mReplay != null) {
            mReplay.PrepareToSimulation();
            Pause();
        }
    }

    void Update() {
        if (enabled && !mReplay.PlayNextFrame(Time.deltaTime) && OnReplayEnd != null) {
            enabled = false;
            OnReplayEnd.Invoke(this, new EventArgs());
        }
    }
}
