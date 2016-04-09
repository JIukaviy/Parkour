using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class IKTargetUI : MonoBehaviour, IDragHandler {
    IK.IKTarget mTarget;
    RectTransform rectTransform;
    Skeleton mSkeleton;

    public IK.IKTarget IKTarget {
        get { return mTarget; }
        set {
            if (mTarget != null) {
                mTarget.OnPositionChange -= new EventHandler<IK.IKTarget.IKTargetEventArgs>(OnTargetPosChanged);
            }
            mTarget = value;
            mTarget.OnPositionChange += new EventHandler<IK.IKTarget.IKTargetEventArgs>(OnTargetPosChanged);
        }
    }

    public Skeleton skeleton {
        get { return mSkeleton; }
        set { mSkeleton = value; }
    }

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnTargetPosChanged(object sender, IK.IKTarget.IKTargetEventArgs e) {
        rectTransform.position = Camera.main.WorldToScreenPoint(e.position);
    }

    public void OnDrag(PointerEventData eventData) {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mTarget.TryMoveTo(mTarget.position + Vector2.ClampMagnitude(worldPosition - mTarget.position, 0.1f));
        mSkeleton.UpdateSkeleton();
    }
}
