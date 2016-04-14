using UnityEngine;
using System.Collections;
using System;

public class ReplayPlayer : MonoBehaviour {
    public EventHandler OnReplayEnd;

    Replay mCurrentReplay;
    bool mPlayingReplay;

    public void Play(Replay aReplay) {
        mCurrentReplay = aReplay;
        mCurrentReplay.SetObjectsIsKinematic(true);
        mPlayingReplay = true;
    }

    public void Pause() {
        mPlayingReplay = false;
    }

    public void Continue() {
        if (mCurrentReplay != null) {
            mPlayingReplay = true;
        }
    }

    public void Stop() {
        mPlayingReplay = false;
        if (mCurrentReplay != null) {
            mCurrentReplay.SetObjectsIsKinematic(false);
            mCurrentReplay = null;
        }
    }

    public void ToStart() {
        if (mCurrentReplay != null) {
            mCurrentReplay.ToStart();
            mPlayingReplay = false;
        }
    }

    void Update() {
        if (mPlayingReplay && !mCurrentReplay.PlayNextFrame(Time.deltaTime) && OnReplayEnd != null) {
            OnReplayEnd.Invoke(this, new EventArgs());
        }
    }
}
