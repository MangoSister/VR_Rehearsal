using UnityEngine;
using UnityEngine.UI;
using System;

public class SwitchFlagController : MonoBehaviour {
    public GameObject btnTransition, grpTransition, btnPause, grpPause;


	// Use this for initialization
	void Start () {
	
	}
	
    public void toTransition()
    {
        btnTransition.SetActive(true);
        grpTransition.SetActive(true);
        btnPause.SetActive(false);
        grpPause.SetActive(false);
    }

    public void toPause()
    {
        btnTransition.SetActive(false);
        grpTransition.SetActive(false);
        btnPause.SetActive(true);
        grpPause.SetActive(true);
    }

	// Update is called once per frame
	void Update () {
	    
	}
}
