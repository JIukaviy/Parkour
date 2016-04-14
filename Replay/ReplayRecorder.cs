using UnityEngine;
using System.Collections.Generic;

public class ReplayRecorder : MonoBehaviour {
    bool mRecordingReplay;
    Replay mCurrentReplay;

    float mCurrentFrameTime;
    float mTimeDelta;

    public float timeDelta {
        get { return mTimeDelta; }
        set { mTimeDelta = value; }
    }

    public void StartRecording(GameObject[] aGameObjects) {
        mCurrentReplay = new Replay(aGameObjects);
        mRecordingReplay = true;
    }

    public void StartRecording(List<GameObject> aGameObjects) {
        StartRecording(aGameObjects.ToArray());
    }

    public void StartRecording(string aObjectsTag) {
        StartRecording(GameObject.FindGameObjectsWithTag(aObjectsTag));
    }

    public void PauseRecording() {
        mRecordingReplay = false;
    }

    public void ContinueRecording() {
        if (mCurrentReplay != null) {
            mRecordingReplay = true;
        }
    }

    public Replay StopRecording() {
        mRecordingReplay = false;
        mCurrentFrameTime = 0;
        return mCurrentReplay;
    }

    void Update() {
        if (mRecordingReplay) {
            mCurrentFrameTime += Time.deltaTime;
            if (mCurrentFrameTime >= mTimeDelta) {
                mCurrentReplay.RecordFrame(mCurrentFrameTime);
                mCurrentFrameTime -= mTimeDelta;
            }
        }
    }
}
