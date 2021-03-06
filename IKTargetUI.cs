﻿using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class IKTargetUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler {
    public EventHandler OnDown;
    public EventHandler OnUp;
    public EventHandler OnPosChanged;

    IK.IKTarget mTarget;
    RectTransform mTransform;
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
        mTransform = GetComponent<RectTransform>();
    }

    public void OnTargetPosChanged(object sender, IK.IKTarget.IKTargetEventArgs e) {
        mTransform.position = Camera.main.WorldToScreenPoint(e.position);
        if (OnPosChanged != null) {
            OnPosChanged.Invoke(this, new EventArgs());
        }
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData) {
        if (gameObject.activeSelf) {
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mTarget.TryMoveTo(mTarget.position + Vector2.ClampMagnitude(worldPosition - mTarget.position, 0.1f));
            mSkeleton.UpdateSkeleton();
        }
    }

    public void Update() {
        mTransform.position = Camera.main.WorldToScreenPoint(mTarget.position);
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (OnDown != null) {
            OnDown.Invoke(this, new EventArgs());
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (OnUp != null) {
            OnUp.Invoke(this, new EventArgs());
        }
    }
}
