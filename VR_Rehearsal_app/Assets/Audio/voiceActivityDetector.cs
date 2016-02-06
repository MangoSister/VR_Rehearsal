/*
 * Chris sun 
 * update: jan 24, 2016
 * 
 * checking voice activity 
*/

using UnityEngine;
using System;
using System.Collections;

public class voiceActivityDetector  {

    //<20 = idle threshold
    //PC: 20
    //mobile : 75 ~ 100
#if UNITY_EDITOR
    private const float THRESHOLD = 5.0f;
#elif UNITY_ANDROID
    private const float THRESHOLD = 75.0f;
#endif
    private AudioClip _clip;

    private float[] _sampleBuffer;

    public voiceActivityDetector(AudioClip clip)
    {
        _clip = clip;
        _sampleBuffer = new float[_clip.frequency * ((int)_clip.length /*+ 1*/)];
    }

    public bool CheckActivity()
	{
		//get volume, 1sec = FREQUENCY samples 

		int arrayLength = Convert.ToInt32(_clip.frequency * Time.deltaTime);
		int startPoint = Convert.ToInt32(Microphone.GetPosition(null));

        _clip.GetData(_sampleBuffer, 0);

		//get average
		float sum = 0f;
		for (int i = (startPoint - arrayLength > 0) ? (-arrayLength) : (-startPoint); i < arrayLength; i++)
		{
			sum += Mathf.Abs(_sampleBuffer[i + startPoint]);
		}
		float avg = 500.0f * sum / arrayLength;

		if (avg < THRESHOLD )
		{
			return false; // idle
		}
		else
		{
			return true; //speaking
		}
	}


}
