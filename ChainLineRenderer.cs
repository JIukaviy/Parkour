using UnityEngine;
using System.Collections.Generic;

public class ChainLineRenderer : MonoBehaviour {

    LineRenderer mRenderer;
    Skeleton.Bone mBone;
    float mWidth;

    void Start() {
        gameObject.AddComponent<LineRenderer>();
        mRenderer = gameObject.GetComponent<LineRenderer>();
    }

    void Update() {
        mRenderer.SetPosition(0, new Vector3(mBone.startPoint.x, mBone.startPoint.y, -1));
        mRenderer.SetPosition(1, new Vector3(mBone.endPoint.x, mBone.endPoint.y, -1));
        mRenderer.SetWidth(mWidth, mWidth);
    }

    public static void ApplyLineRenderer(Skeleton.Bone Bone, float Width) {
        GameObject gameObject = new GameObject();

        gameObject.AddComponent<ChainLineRenderer>();
        ChainLineRenderer chainRenderer = gameObject.GetComponent<ChainLineRenderer>();
        chainRenderer.mBone = Bone;
        chainRenderer.mWidth = Width;
               
        foreach(Skeleton.Bone bone in Bone.childs) {
            ApplyLineRenderer(bone, Width);
        }
    }
}
