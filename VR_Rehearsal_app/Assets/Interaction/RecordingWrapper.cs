using UnityEngine;
using System;
using System.IO;
using System.Collections;
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

    //Return cumulative time record of the speaker's fluency (from voice plugin)
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

            int acc = 0;
            for (int i = 0; i < result.Count; ++i)
            {
                int oldVal = result[i].Value;
                result[i] = new KeyValuePair<bool, int>(result[i].Key, acc + result[i].Value);
                acc += oldVal;
            }
                      
            return result;
        }
    }

#if UNITY_EDITOR
    private List<KeyValuePair<bool, int>> _mockList;
#endif

    public void Init()
    {
        recordingFilePath = Application.persistentDataPath + "/record.pcm";
        fluencyRecord = new List<KeyValuePair<bool, int>>();
#if !UNITY_EDITOR && UNITY_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
#else
        _mockList = new List<KeyValuePair<bool, int>>();
#endif
    }

    public void StartRecording()
    {
        if (File.Exists(recordingFilePath))
            File.Delete(recordingFilePath);

#if !UNITY_EDITOR && UNITY_ANDROID
        //currentActivity.Call("finish");
        //currentActivity.Call("recreate");
		currentActivity.Call("initialize_recordNplayback", recordingFilePath, PresentationData.in_VoiceThreshold);
        currentActivity.Call("setReverbStrength", reverbStrength);
#else
        StartCoroutine(MockVoiceDetect_CR());
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
            (Convert.ToInt32(status) != 0 ? true : false,
            Convert.ToInt32(time));

            statusQueue.Enqueue(pair);
        }

        return statusQueue;
#else
        //mock-up
        var statusQueue = new Queue<KeyValuePair<bool, int>>(_mockList);
        _mockList.Clear();
        return statusQueue;
#endif
    }


    public bool EndRecording()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if(currentActivity != null)
        {
            currentActivity.Call("prepareReplay");
            return true;
        }
        else return false;
#else
        StopAllCoroutines();
        return true;
#endif
    }

    public void UpdateFluencyScore()
    {
        if (debugText != null)
            debugText.text = string.Empty;

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
#if UNITY_EDITOR
            print(string.Format("(ratio: {0})", fluencyFactor));
#endif
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

    }

    public bool EarphonePlugged()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        return currentActivity.Call<bool>("checkHeadsetPlugged");
#else
        return true;
#endif

    }

#if UNITY_EDITOR
    private IEnumerator MockVoiceDetect_CR()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.S))
            {
                if (_mockList.Count == 0)
                {
                    _mockList.Add(new KeyValuePair<bool, int>(true, Mathf.FloorToInt(1000 * Time.deltaTime)));
#if UNITY_EDITOR
                    print("Speaking");
#endif
                }
                else
                {
                    if (_mockList[_mockList.Count - 1].Key == false)
                    {
                        _mockList.Add(new KeyValuePair<bool, int>(true, Mathf.FloorToInt(1000 * Time.deltaTime)));
#if UNITY_EDITOR
                        print("Speaking");
#endif
                    }
                    else
                        _mockList[_mockList.Count - 1] = new KeyValuePair<bool, int>(true,
                            _mockList[_mockList.Count - 1].Value + Mathf.FloorToInt(1000 * Time.deltaTime));
                }
            }
            else
            {
                if (_mockList.Count == 0)
                {
                    _mockList.Add(new KeyValuePair<bool, int>(false, Mathf.FloorToInt(1000 * Time.deltaTime)));
#if UNITY_EDITOR
                    print("Silent");
#endif
                }
                else
                {
                    if (_mockList[_mockList.Count - 1].Key == true)
                    {
                        _mockList.Add(new KeyValuePair<bool, int>(false, Mathf.FloorToInt(1000 * Time.deltaTime)));
#if UNITY_EDITOR
                        print("Silent");
#endif
                    }
                    else
                        _mockList[_mockList.Count - 1] = new KeyValuePair<bool, int>(false,
                            _mockList[_mockList.Count - 1].Value + Mathf.FloorToInt(1000 * Time.deltaTime));
                }
            }

            yield return null;
        }

    }
#endif
}
