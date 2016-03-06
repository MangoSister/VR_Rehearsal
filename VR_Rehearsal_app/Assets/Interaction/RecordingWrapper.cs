using UnityEngine;

public class RecordingWrapper : MonoBehaviour
{
    [Range(0f, 1f)]
    public float reverbStrength = 0.2f;

    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;

#if !UNITY_EDITOR
    public void StartRecording()
    {
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        currentActivity.Call("initialize_recordNplayback", Application.persistentDataPath);
        currentActivity.Call("setReverbStrength", reverbStrength);
    }

    public void CaptureVoiceStatus()
    {
        string json = currentActivity.Call<string>("getRecord", reverbStrength);
        SimpleJSON.JSONNode parseResult = SimpleJSON.JSON.Parse(json);

        for (int idx = 0; idx < parseResult["entries"].Count; ++idx)
        {
            var status = parseResult["entries"][idx]["status"].Value; //int 0: silence; 1: speaking
            var time = parseResult["entries"][idx]["time"].Value; //int (ms)
        }
    }

    public void EndRecording()
    {
        currentActivity.Call<string>("prepareReplay");
    }


#else
    public void StartRecording()
    {
    }

    public void CaptureVoiceStatus()
    {
    }

    public void EndRecording()
    {
    }

#endif
}
