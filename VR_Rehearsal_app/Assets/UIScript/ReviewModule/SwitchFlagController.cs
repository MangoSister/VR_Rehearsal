using UnityEngine;
using UnityEngine.UI;
using System;

public class SwitchFlagController : MonoBehaviour {
    public GameObject btnTransition, grpTransition, btnPause, grpPause;
    public Sprite transitionOn, transitionOff, pauseOn, pauseOff;
    private bool isTransition = false, isPause = false;


	// Use this for initialization
	void Start () {
	}
	
    public void triggerTransition()
    {
        if (isTransition == false)
        {
            btnTransition.GetComponent<Image>().sprite = transitionOn;
            grpTransition.SetActive(true);
            isTransition = true;
        }
        else
        {
            btnTransition.GetComponent<Image>().sprite = transitionOff;
            grpTransition.SetActive(false);
            isTransition = false;
        }
    }

    public void triggerPause()
    {
        if (isPause == false)
        {
            btnPause.GetComponent<Image>().sprite = pauseOn;
            grpPause.SetActive(true);
            isPause = true;
        }
        else
        {
            btnPause.GetComponent<Image>().sprite = pauseOff;
            grpPause.SetActive(false);
            isPause = false;
        }
    }

	// Update is called once per frame
	void Update () {
	    
	}
}
