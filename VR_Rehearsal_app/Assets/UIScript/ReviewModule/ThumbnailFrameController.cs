using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThumbnailFrameController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    public void SetFrameVisible(bool flag)
    {
        //gameObject.SetActive(flag);
        if (flag == false)
            gameObject.GetComponent<Transform>().localPosition = new Vector3(700, 0, 0);
        else
            gameObject.GetComponent<Transform>().localPosition = new Vector3(0, 0, 0);                
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
