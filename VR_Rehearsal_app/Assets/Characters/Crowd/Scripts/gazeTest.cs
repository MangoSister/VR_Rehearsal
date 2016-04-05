using UnityEngine;
using System.Collections;

public class gazeTest : MonoBehaviour {

	public Transform targetTransform;
	public Transform HeadWrapper;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		HeadWrapper.LookAt (targetTransform);
	}
}
