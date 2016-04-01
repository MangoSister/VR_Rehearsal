using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class RecordingWrapper : MonoBehaviour
{
    [Range(0f, 1f)]
    public float reverbStrength = 0.3f;

    public float fluencyFactor = 0f;

    public float fluencyDelta = 0f;
#if !UNITY_EDITOR && UNITY_ANDROID
    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;
#else 
    //Debug purpose, to fake the result from voice plugin
    [Range(0f, 1f)]
    public float fakeFluencyFactor;
#endif
    //Debug purpose
    public TextMesh debugText;

    public string recordingFilePath;

    private List<KeyValuePair<bool, int>> fluencyRecord;
    public List<KeyValuePair<bool, int>> outputFluencyRecord
    {
        get
        {
            var result = new List<KeyValuePair<bool, int>>();
            if (fluencyRecord != null && fluencyRecord.Count > 0)
            {
                bool currState = fluencyRecord[0].Key;
                int currLength = 0;
                foreach (var pair in fluencyRecord)
                {
                    if (pair.Key != currState)
                    {
                        result.Add(new KeyValuePair<bool, int>(currState, currLength));
                        currState = pair.Key;
                        currLength = pair.Value;
                    }
                    else
                        currLength += pair.Value;
                }
                result.Add(new KeyValuePair<bool, int>(currState, currLength));
            }           
            return result;
        }
    }

    private void Awake()
    {
        recordingFilePath = Application.persistentDataPath + "/record.pcm";
        fluencyRecord = new List<KeyValuePair<bool, int>>();
    }

    public void StartRecording()
    {
        if (File.Exists(recordingFilePath))
            File.Delete(recordingFilePath);
#if !UNITY_EDITOR && UNITY_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call("initialize_recordNplayback", (recordingFilePath));
        currentActivity.Call("setReverbStrength", reverbStrength);
#endif
    }

    private Queue<KeyValuePair<bool, int>> CaptureVoiceStatus()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        string json = currentActivity.Call<string>("getRecord");
        SimpleJSON.JSONNode parseResult = SimpleJSON.JSON.Parse(json);
        Queue<KeyValuePair<bool, int>> statusQueue = new Queue<KeyValuePair<bool, int>>();

        for (int idx = 0; idx < parseResult["status"].Count; ++idx)
        {
            var status = parseResult["status"][idx]["status"].Value; //int 0: silence; 1: speaking
            var time = parseResult["status"][idx]["time"].Value; //int (ms)

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
        if (debugText != null)
            debugText.text = string.Empty;
#if !UNITY_EDITOR && UNITY_ANDROID
        Queue<KeyValuePair<bool, int>> statusQueue = CaptureVoiceStatus();
        float oldFactor = fluencyFactor;

        int speakingLength = 0, silenceLength = 0;
        if (statusQueue.Count == 0)
        {
            fluencyFactor = 0f;
            fluencyDelta = 0f;
            if (debugText != null)
                debugText.text += "no data " + fluencyFactor;
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
            if (debugText != null)
                debugText.text += fluencyFactor;
            fluencyFactor = CrowdSimulator.currSim.fluencyCurve.Evaluate(fluencyFactor);
            fluencyFactor = Mathf.Clamp(fluencyFactor, -1f, 1f);
            fluencyDelta = fluencyFactor - oldFactor;
            if (debugText != null)
            {
                debugText.text += "\n" + fluencyFactor;
                debugText.text += "\n" + "delta: " + fluencyDelta;
            }
            
            fluencyRecord.AddRange(statusQueue);
        }
#else
        float oldFactor = fluencyFactor;
        fluencyFactor = fakeFluencyFactor;
        if (debugText != null)
            debugText.text += fluencyFactor;
        fluencyFactor = CrowdSimulator.currSim.fluencyCurve.Evaluate(fluencyFactor);
        fluencyFactor = Mathf.Clamp(fluencyFactor, -1f, 1f);
        fluencyDelta = fluencyFactor - oldFactor;
        if (debugText != null)
        {
            debugText.text += "\n" + fluencyFactor;
            debugText.text += "\n" + "delta: " + fluencyDelta;
        }
#endif

    }
}
