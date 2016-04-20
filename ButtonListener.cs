using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class ButtonListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public EventHandler OnPress;
    public EventHandler OnRelease;

    public void OnPointerDown(PointerEventData eventData) {
        if (OnPress != null) {
            OnPress.Invoke(this, new EventArgs());
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (OnRelease != null) {
            OnRelease.Invoke(this, new EventArgs());
        }
    }
}
