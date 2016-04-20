using UnityEngine;
using System.Collections;

public class RopeCreator : MonoBehaviour {
    public int RopeLength;
    public GameObject ChainElemetPrefab;

    GameObject[] mRopeElements;
    
	void Start() {
        mRopeElements = new GameObject[RopeLength];
        Rigidbody2D parent = gameObject.GetComponent<Rigidbody2D>();
        Vector3 startPoint = GetComponent<Transform>().position;
        for (int i = 0; i < RopeLength; i++) {
            GameObject newChainElement = Instantiate(ChainElemetPrefab);

            Transform newChainElementTransform = newChainElement.GetComponent<Transform>();
            newChainElementTransform.position = startPoint + newChainElementTransform.position - newChainElementTransform.TransformPoint(new Vector2(0, 0.5f));
            startPoint -= newChainElementTransform.position - newChainElementTransform.TransformPoint(new Vector2(0, -0.5f));

            HingeJoint2D joint = newChainElement.AddComponent<HingeJoint2D>();
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = new Vector2(0, 0.5f);
            joint.connectedBody = parent.GetComponent<Rigidbody2D>();
            joint.connectedAnchor = new Vector2(0, -0.5f);
            parent = newChainElement.GetComponent<Rigidbody2D>();
            mRopeElements[i] = newChainElement;
        }
	}

    public GameObject[] GetChainElements() {
        return mRopeElements;
    }
}
