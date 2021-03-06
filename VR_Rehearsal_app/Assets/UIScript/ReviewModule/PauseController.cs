﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

//Chris Sun
//For jump to Pause tags in the replay slider

public class PauseController : MonoBehaviour {

    private AudioSource audioSource;
    private Slider playbackSlider;
    public float time = 0.0f; //in seconds

    public void jumpTo() //timeStamp is in ms, since the beginning of the talk
    {
        //UnityEngine.Debug.Log("This is called");

        playbackSlider = GameObject.FindObjectOfType<Slider>();
        audioSource = GameObject.FindObjectOfType<AudioSource>();

        if (audioSource != null)
        {
            //UnityEngine.Debug.Log("try to jump to " + (time).ToString("0.00") + " of " + playbackSlider.maxValue);
            GameObject.FindObjectOfType<ReplayController>().preventTrigger = true;
            playbackSlider.value = time;
            audioSource.time = playbackSlider.value;
            GameObject.FindObjectOfType<ReplayController>().preventTrigger = false;
                
            if (audioSource.isPlaying != true)
            {
                audioSource.Play();
            }
        }
        else
            UnityEngine.Debug.Log("cannot find an audiosource");
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
