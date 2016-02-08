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

    public bool CheckActivity() //have to check 1 frame before
    {
        //get volume, 1sec = FREQUENCY samples 
        AudioClip rec = _clip;
        if (rec == null)
        {
            UnityEngine.Debug.Log("Fail to fetch audio source");
            return false;
        }

        rec.GetData(_sampleBuffer, 0);

        //get average
        float sum = 0f;

        //wrapped or not
        int frameLength = Convert.ToInt32(_clip.frequency * Time.deltaTime);
        int currentPoint = Convert.ToInt32(Microphone.GetPosition(null));
        //UnityEngine.Debug.Log(currentPoint);

        if ((currentPoint - 2 * frameLength) > 0) //2nd last frame at (currentPoint-2frameLength ~ currentPoint-frameLength)
        {
            for (int i = currentPoint - 2 * frameLength; i < currentPoint - frameLength; i++)
            {
                sum += Mathf.Abs(_sampleBuffer[i]);
            }
        }
        else
        {
            if (currentPoint - frameLength > 0)
            {
                int offsetTillEnd = 2 * frameLength - currentPoint;
                int startIndex = _clip.frequency * (int)_clip.length - offsetTillEnd;
                int endIndex = currentPoint - frameLength;
                //UnityEngine.Debug.Log("(a) length = " + (FREQUENCY * LISTEN_INTERVAL - startIndex) + "+" + endIndex + ", frameLength = " + frameLength);
                for (int i = startIndex; i < _clip.frequency * (int)_clip.length; i++)
                {
                    sum += Mathf.Abs(_sampleBuffer[i]);
                }
                for (int i = 0; i < endIndex; i++)
                {
                    sum += Mathf.Abs(_sampleBuffer[i]);
                }
            }
            else
            {
                int offsetTillEnd = frameLength - currentPoint;
                int startIndex = _clip.frequency * (int)_clip.length - offsetTillEnd - frameLength;
                int endIndex = startIndex + frameLength;
                //UnityEngine.Debug.Log("(b) length = " + (endIndex - startIndex) + ", frameLength = " + frameLength);
                for (int i = startIndex; i < endIndex; i++)
                {
                    sum += Mathf.Abs(_sampleBuffer[i]);
                }
            }

        }
        float avg = 500.0f * sum / frameLength;
       // volume.text = "Volume: " + avg.ToString("0.00");
        if (avg < THRESHOLD)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}
