#if UNITY_ANDROID && !UNITY_EDITOR
#define USE_ANDROID
#endif
using UnityEngine;
using System;

using System.Collections;
using UnityEngine.UI;

public class CalibrationView : MonoBehaviour
{
	public static bool isCalibrationDone;

    enum Status {Begin =0, Silence = 1, Talking =2, Done =3, showText = 4 };
    int currentStatus;

	public GameObject popUpWindow;
	public GameObject button;
    public GameObject calibrtaionData;
	public GameObject contentText;
	public GameObject descriptionPanel;
	public GameObject circularProgress;
    public Text debugText;
	public GameObject mainIcon_silence;
    public GameObject mainIcon_say;
    public GameObject doneButton;
    public GameObject resetButton;

	float rate = 20f;
	float max_time = 100f;
	float curr_time = 0f;
	float calc_timer;
    int stage = 0;

    bool updateVolumeFlag = false; //if true, then update volume every frame

    //for calling unity android activity
    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;

    private int avgSilence, avgSpeaking, threshold=0;
    bool isButtonClicked;
    bool isFlag;
	// Use this for initialization
	void Start ()
	{
        mainIcon_say.SetActive(false);
        isCalibrationDone = false;
		descriptionPanel.SetActive(true);
		circularProgress.SetActive(true);
		curr_time = 0;
        currentStatus = 0;
        isButtonClicked = false;
        isFlag = false;
        debugText.text = "Loading";
        //setup Unity Activity
#if USE_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
#endif
        debugText.text = "Ready";
	}

    // Update is called once per frame
    void Update()
    {
        if (currentStatus == (int)Status.Done)
        {
            updateVolumeFlag = true;
            button.GetComponent<Button>().interactable = false;
        }
        if (isButtonClicked == true)
        {
            IncreaseTimer();
        }
        if(currentStatus == (int)Status.Silence && isButtonClicked == false)
        {
            contentText.GetComponent<Text>().text = "Please say something...";
            mainIcon_silence.SetActive(false);
            mainIcon_say.SetActive(true);
            currentStatus++;
        }

        if (updateVolumeFlag == true && stage == 2)
        {
#if USE_ANDROID
            int volume = currentActivity.Call<int>("getNowAvg");

            if (volume > (threshold * 1.5))
                calibrtaionData.GetComponentInChildren<Text>().text = "<color=red>" + volume + "</color>";
            else if (volume > (threshold * 0.9))
                calibrtaionData.GetComponentInChildren<Text>().text = volume.ToString();
            else if (volume > (threshold * 0.7))
                calibrtaionData.GetComponentInChildren<Text>().text = "<color=yellow>" + volume + "</color>";
            else
                calibrtaionData.GetComponentInChildren<Text>().text = "<color=grey>" + volume + "</color>";
#endif
        }
        
        else {
            calibrtaionData.GetComponentInChildren<Text>().text = "Calibration Start";
        }
        
    }

    void IncreaseTimer ()
	{
		if (curr_time < 100) {
			curr_time += (Time.deltaTime * rate);
			circularProgress.GetComponent<RectTransform>().FindChild("loading").GetComponent<Image>().fillAmount = curr_time/max_time;
			button.GetComponent<Button>().interactable = false;
		}
        else
        {
            stage += 1;
            curr_time = 0;
			circularProgress.GetComponent<RectTransform>().FindChild("loading").GetComponent<Image>().fillAmount =  0;
            if(currentStatus != (int)Status.Done)
            {
#if USE_ANDROID
        debugText.text = (currentActivity.Call<int>("stopTestThreshold")).ToString();
#endif
            }
#if USE_ANDROID
            avgSilence = Convert.ToInt32(debugText.text);
#endif
            button.GetComponent<Button>().interactable = true;
            isButtonClicked = false;
            if(currentStatus == (int)Status.Done)
            {
                currentStatus += 1;
            }
        }
	}
		
	public void PopUpOKButtonClick ()
	{
		popUpWindow.SetActive (false);
		descriptionPanel.SetActive(true);
		circularProgress.SetActive(true);
        circularProgress.GetComponent<RectTransform>().SetAsLastSibling();
    }

	public void PopUpSkipButtonClick ()
	{
		popUpWindow.SetActive (false);
		gameObject.SetActive (false);
		isCalibrationDone = true;
	}
    
    public void CalirbartionStartButtonClick()
    {
        //First Button Click
        if (currentStatus == (int)Status.Begin) {
            Debug.Log("first Silent button Clicked...");
#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
#endif
            currentStatus+=1;
            isButtonClicked = true;
        }
    
		//Second Button Click
	else if(currentStatus == (int)Status.Talking) { 
            Debug.Log("2nd Talking button Clicked");
#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
#endif
            isButtonClicked = true;
#if USE_ANDROID
            avgSilence = Convert.ToInt32(debugText.text);
#endif
            threshold = avgSilence + (avgSpeaking - avgSilence) / 2;
            //updateVolumeFlag = true;
            isFlag = true;
            currentStatus+=1;
        }
        else if(currentStatus == (int)Status.showText)
        {
            button.GetComponent<Button>().interactable = false;
        }

            
        /*
        contentText.GetComponent<Text>().text = "see if it workes properly!";
#if USE_ANDROID
        currentActivity.Call("startTestThreshold");
#endif
        updateVolumeFlag = true;
    }
    */
    }

    void ChangeTheText ()
	{
     button.GetComponentInChildren<Text> ().text = "Done !";
#if USE_ANDROID
        debugText.text = (currentActivity.Call<int>("stopTestThreshold")).ToString();
#endif
	}

    public void DoneButtonClick()
    {
        if (currentStatus == (int)Status.showText)
        {
            Debug.Log("threshold = " + threshold);
            calibrtaionData.GetComponent<Text>().text = threshold.ToString();
            gameObject.SetActive(false);

#if USE_ANDROID
        debugText.text = (currentActivity.Call<int>("stopTestThreshold")).ToString();
#endif
            PresentationData.in_VoiceThreshold = threshold;
            isCalibrationDone = true;
        }
    }

    public void ResetButtonClicked()
    {
        if (currentStatus == (int)Status.showText)
        {
#if USE_ANDROID
        currentActivity.Call<int>("stopTestThreshold");
#endif
            currentStatus = 0;
            isFlag = false;
            isButtonClicked = false;
            stage = 0;
            curr_time = 0;
            if (currentStatus != (int)Status.Begin)
            {
                currentStatus = (int)Status.Begin;
            }
            button.GetComponent<Button>().interactable = true;
        }
    }
}
