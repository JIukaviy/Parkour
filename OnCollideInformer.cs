using UnityEngine;
using System;

//Временный класс для тестирования
public class OnCollideInformer : MonoBehaviour {
    public EventHandler OnFinish;
    public string LayerName;

    int mLayer;

    void Start() {
        mLayer = LayerMask.NameToLayer(LayerName);
    }

    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.layer == mLayer && OnFinish != null) {
            OnFinish.Invoke(this, new EventArgs());
        }
    }
}
