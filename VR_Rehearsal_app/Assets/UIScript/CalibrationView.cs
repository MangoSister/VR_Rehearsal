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

	public GameObject popUpWindow;
	public GameObject button;
	public GameObject contentText;
	public GameObject descriptionPanel;
	public GameObject circularProgress;
    public Text debugText;
	public GameObject mainIcon;

	float rate = 20f;
	float max_time = 100f;
	float curr_time = 0f;
	float calc_timer;

	bool silentFlag = false;
    bool updateVolumeFlag = false; //if true, then update volume every frame
	bool isSilentCalibrationDone = false;
	bool isMicroCalibrationDone = false;
	bool isBetwwen = false;

    //for calling unity android activity
    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;

    private int avgSilence, avgSpeaking, threshold=0;

	// Use this for initialization
	void Start ()
	{
        UnityEngine.Debug.Log("initializing");
        
        isCalibrationDone = false;
		descriptionPanel.SetActive(false);
		circularProgress.SetActive(false);
		curr_time = 0;

        UnityEngine.Debug.Log("setup unity activity");
        debugText.text = "Loading";
        //setup Unity Activity
		#if USE_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		#endif
        debugText.text = "Ready";
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (silentFlag == true) {
			IncreaseTimer ();
		}

        if (updateVolumeFlag == true)
        {
            int volume = currentActivity.Call<int>("getNowAvg");

            if (volume > (threshold * 1.2))
                button.GetComponentInChildren<Text>().text = "<color=red>" + volume + "</color>";
            else if (volume > (threshold * 0.9))
                button.GetComponentInChildren<Text>().text = volume.ToString();
            else if (volume > (threshold * 0.7))
                button.GetComponentInChildren<Text>().text = "<color=yellow>" + volume + "</color>";
            else
                button.GetComponentInChildren<Text>().text = "<color=grey>" + volume + "</color>";
        }
        else
            button.GetComponentInChildren<Text>().text = "Calibration Start";
	}

	void IncreaseTimer ()
	{
		if (curr_time < 100) {
			curr_time += (Time.deltaTime * rate);
			circularProgress.GetComponent<RectTransform>().FindChild("loading").GetComponent<Image>().fillAmount = curr_time/max_time;
			button.GetComponent<Button>().interactable = false;

		} else {
			circularProgress.GetComponent<RectTransform>().FindChild("loading").GetComponent<Image>().fillAmount =  0;
			silentFlag = false;
			button.GetComponent<Button>().interactable = true;
			ChangeTheText ();
		}
		isSilentCalibrationDone = true;
	}
		
	public void PopUpOKButtonClick ()
	{
		popUpWindow.SetActive (false);
		descriptionPanel.SetActive(true);
		circularProgress.SetActive(true);
	}

	public void PopUpSkipButtonClick ()
	{
		popUpWindow.SetActive (false);
		gameObject.SetActive (false);
		isCalibrationDone = true;
	}

	public void CalirbartionStartButtonClick ()
	{
		//First Silence button
		if (isSilentCalibrationDone == false && isMicroCalibrationDone == false && button.GetComponentInChildren<Text> ().text == "Calibration Start") {
			if (silentFlag == false) {
				silentFlag = true;
			}
			#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
			#endif
		} 
		//First Done Button
		else if (isSilentCalibrationDone == true && isMicroCalibrationDone == false && button.GetComponentInChildren<Text> ().text == "Done !" ){
			#if USE_ANDROID
            avgSilence = Convert.ToInt32(debugText.text);
			#endif
            Debug.Log("2");
			button.GetComponentInChildren<Text> ().text = "Calibration Start";
			contentText.GetComponent<Text>().text = "please test your microphone. Say something shit";
		}
		//Microbutton
		else if(isSilentCalibrationDone == true && isMicroCalibrationDone == false && button.GetComponentInChildren<Text> ().text == "Calibration Start"){
			curr_time = 0;
			silentFlag = true;
			isMicroCalibrationDone = true;
			#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
			#endif
		}
		//2nd Done Button
		else if(isSilentCalibrationDone == true && isMicroCalibrationDone == true){

			Debug.Log("Rotation!!!!!");
			popUpWindow.SetActive (false);
			gameObject.SetActive (false);
			isCalibrationDone = true;
			//Debug.Log("Rotation!!!!!");
            //now test volume
			#if USE_ANDROID
            avgSpeaking = Convert.ToInt32(debugText.text);
			#endif
            threshold = avgSilence + (avgSpeaking - avgSilence) / 2;
            Debug.Log("test");
            contentText.GetComponent<Text>().text = "see if it workes properly!";
			#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
			#endif
            updateVolumeFlag = true;
		}
	}

	void ChangeTheText ()
	{
		button.GetComponentInChildren<Text> ().text = "Done !";
		#if USE_ANDROID
        debugText.text = (currentActivity.Call<int>("stopTestThreshold")).ToString();
		#endif
	}
}
