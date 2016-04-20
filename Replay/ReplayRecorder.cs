using UnityEngine;
using System.Collections.Generic;

public class ReplayRecorder : MonoBehaviour {
    Replay mReplay;

    float mCurrentFrameTime;
    float mTimeDelta;

    public float timeDelta {
        get { return mTimeDelta; }
        set { mTimeDelta = value; }
    }

    public bool recording { get { return enabled; } }
    public Replay replay { get { return mReplay; } }

    void Awake() {
        enabled = false;
    }

    public void StartRecording(GameObject[] aGameObjects) {
        mReplay = new Replay(aGameObjects);
        mCurrentFrameTime = 0;
        enabled = true;
    }

    public void StartRecording(List<GameObject> aGameObjects) {
        StartRecording(aGameObjects.ToArray());
    }

    public void StartRecording(string aObjectsTag) {
        StartRecording(GameObject.FindGameObjectsWithTag(aObjectsTag));
    }

    public void PauseRecording() {
        enabled = false;
    }

    public void ContinueRecording() {
        if (mReplay != null) {
            enabled = true;
        }
    }

    public Replay StopRecording() {
        mCurrentFrameTime = 0;
        enabled = false;
        return mReplay;
    }

    public void EraseRecorded() {
        if (mReplay != null) {
            mReplay.EraseRecorded();
            enabled = false;
        }
    }

    public void ConcatenateReplay(Replay aReplay) {
        if (mReplay == null) {
            mReplay = aReplay;
        } else {
            mReplay.ConcatenateReplay(aReplay);
        }
    }

    public void ReplaceGameObjects(Dictionary<GameObject, GameObject> aGameObjects) {
        mReplay.ReplaceGameObjects(aGameObjects);
    }

    void Update() {
        mCurrentFrameTime += Time.deltaTime;
        if (mCurrentFrameTime >= mTimeDelta) {
            mReplay.RecordFrame(mCurrentFrameTime);
            mCurrentFrameTime -= mTimeDelta;
        }
    }
}
