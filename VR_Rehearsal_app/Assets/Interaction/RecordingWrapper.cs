using UnityEngine;
using System;
using System.Collections.Generic;

public class RecordingWrapper : MonoBehaviour
{
    [Range(0f, 1f)]
    public float reverbStrength = 0.3f;

    public float fluencyFactor = 0f;

#if !UNITY_EDITOR && UNITY_ANDROID
    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;
#endif

    public TextMesh debugText;

    public void StartRecording()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call("initialize_recordNplayback", (Application.persistentDataPath + "/record.pcm"));
        //currentActivity.Call("setReverbStrength", reverbStrength);
#endif
    }

    private Queue<KeyValuePair<bool, int>> CaptureVoiceStatus()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        string json = currentActivity.Call<string>("getRecord");
        SimpleJSON.JSONNode parseResult = SimpleJSON.JSON.Parse(json);
        Queue<KeyValuePair<bool, int>> statusQueue = new Queue<KeyValuePair<bool, int>>();

        for (int idx = 0; idx < parseResult["entries"].Count; ++idx)
        {
            var status = parseResult["entries"][idx]["status"].Value; //int 0: silence; 1: speaking
            var time = parseResult["entries"][idx]["time"].Value; //int (ms)

            KeyValuePair<bool, int> pair = new KeyValuePair<bool, int>
            (Convert.ToInt32(status) == 1 ? true : false,
            Convert.ToInt32(time));

            statusQueue.Enqueue(pair);
        }

        return statusQueue;
#else
        return new Queue<KeyValuePair<bool, int>>();
#endif
    }

    public void EndRecording()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        currentActivity.Call<string>("prepareReplay");
#endif
    }

    public void UpdateFluencyScore()
    {
        Queue<KeyValuePair<bool, int>> statusQueue = CaptureVoiceStatus();
        int speakingLength = 0, silenceLength = 0;
        if (statusQueue.Count == 0)
        {
            fluencyFactor = 0f;
        }
        else
        {
            foreach (var status in statusQueue)
            {
                if (status.Key)
                    speakingLength += status.Value;
                else silenceLength += status.Value;
            }


            fluencyFactor = (float)speakingLength / (float)(speakingLength + silenceLength);
            fluencyFactor = CrowdSimulator.currSim.fluencyCurve.Evaluate(fluencyFactor);
            fluencyFactor = Mathf.Clamp(fluencyFactor, -1f, 1f);
        }


        if (debugText != null)
        {
            debugText.text = string.Empty;
            foreach (var status in statusQueue)
            {
                debugText.text += string.Format("{0}, {1}\n", status.Key, status.Value);
            }
            debugText.text += fluencyFactor;
        }
    }
}
