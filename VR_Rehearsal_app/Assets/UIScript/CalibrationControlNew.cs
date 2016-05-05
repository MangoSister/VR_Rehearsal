// VR Rehearsal
// Chris Sun @ 4/11/2016

// Control the new calibration UI

#if UNITY_ANDROID && !UNITY_EDITOR
#define USE_ANDROID
#endif

using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class CalibrationControlNew : MonoBehaviour {
    public static bool isCalibrationDone;

    private int state = 0; //0=testing, 1=silence calibration, 2=voice calibration
    private int threshold = 300; //will be tested and calibrated
    private int tSilence = 0, tSpeaking = 0;
    private float nowTime = 0f, maxTime = 2f;
    private bool isButtonClicked = false;

    //for calling unity android activity
    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;
    public GameObject instructionPanel;
    public GameObject xButon;
    public GameObject questiontext;
    public GameObject questionButton;
    public GameObject localview;
    public GameObject customeView;

    [Header("Change States")]
    public GameObject TestMicrophoneGroup;
    public GameObject TestSilentGroup;
    public GameObject TestTalkingGroup;

    [Header("Check Threshold")]
    public Text txtTestResult;
    public Image btnTestResult;
    public Sprite sprSpeaking;
    public Sprite sprSilent;

    [Header("The Tests")]
    public Image progressBar;
    public GameObject btnStartSilentTest;
    public GameObject btnStartTalkTest;
    public GameObject progressBarGroup;

    //test scene -> silent test -> talking test -> test scene
 
    public void GoToSilent() { //called after test scene
#if USE_ANDROID
        int _useless = currentActivity.Call<int>("stopTestThreshold");
#endif
        btnStartSilentTest.SetActive(true);
        progressBarGroup.SetActive(false);

        TestMicrophoneGroup.SetActive(false);
        TestSilentGroup.SetActive(true);
        TestTalkingGroup.SetActive(false);

        state = 1;
        isButtonClicked = false;
        xButon.SetActive(false);
        questiontext.SetActive(false);

    }

    public void GoToTalking() { //called after test scene
//#if USE_ANDROID
//        int _useless = currentActivity.Call<int>("stopTestThreshold");
//#endif

        btnStartTalkTest.SetActive(true);
        TestMicrophoneGroup.SetActive(false);
        TestSilentGroup.SetActive(false);
        TestTalkingGroup.SetActive(true);

        state = 2;
		isButtonClicked = false;
    }

    public void startTesting()
    {
        btnStartSilentTest.SetActive(false);
        btnStartTalkTest.SetActive(false);
        progressBarGroup.SetActive(true);

#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
#endif
        nowTime = 0f;
        isButtonClicked = true;
    }

    public void GoToTest() { //called after test scene
//#if USE_ANDROID
//        int _useless = currentActivity.Call<int>("stopTestThreshold");
//#endif

        if (state == 2) //update threshold if coming back from talking scene;
            threshold = (tSilence + (tSpeaking - tSilence) / 2) * 3 / 2;
        else //first time 
        {
            if ((PresentationData.in_VoiceThreshold != null) && (PresentationData.in_VoiceThreshold > 0))
                threshold = PresentationData.in_VoiceThreshold;
        }

        TestMicrophoneGroup.SetActive(true);
        TestSilentGroup.SetActive(false);
        TestTalkingGroup.SetActive(false);
        
        state = 0;

        //start the testing
#if USE_ANDROID
            currentActivity.Call("startTestThreshold");
#endif
    }

    public void GoToRotate() { //leave calibration
        state = 0;

#if USE_ANDROID
        int _useless = currentActivity.Call<int>("stopTestThreshold");
#endif
        PresentationData.in_VoiceThreshold = threshold;
        isCalibrationDone = true; //this pass the logic to next scene
    }

	// Use this for initialization
	void Start () {
        ApplicationChrome.navigationBarState = ApplicationChrome.States.VisibleOverContent;
        ApplicationChrome.statusBarState = ApplicationChrome.States.Visible;
        isCalibrationDone = false;
        state = -1;
        threshold = 300;
        instructionPanel.SetActive(false);
#if USE_ANDROID
        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
#endif

        GoToTest();
	}

    void IncreaseTimer()
    {
        if (nowTime<maxTime)
        {
            nowTime += Time.deltaTime;
            progressBar.GetComponent<RectTransform>().sizeDelta = new Vector2((int)((1.0f - nowTime/maxTime) * 800.0f), 10);
        }
        else
        {
            progressBarGroup.SetActive(false);
           int result = 300;
#if USE_ANDROID
            result = currentActivity.Call<int>("stopTestThreshold");
#endif
            if (state == 1)
            { 
                tSilence = result;
                GoToTalking();
            }
            else if (state == 2)
            {
                tSpeaking = result;
                GoToTest();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 0)
        {
            xButon.SetActive(true);
            questiontext.SetActive(true);
            questionButton.SetActive(true);
#if USE_ANDROID
            int volume = currentActivity.Call<int>("getNowAvg");

            if (volume > threshold)
            {
                txtTestResult.text = "<color=#01BAEA>You are speaking.</color>";
                btnTestResult.sprite = sprSpeaking;
            }
            else
            {
                txtTestResult.text = "<color=#01BAEA>You are not speaking.</color>";
                btnTestResult.sprite = sprSilent;
            }
#endif
        }

        if (state == 1)
        {
            instructionPanel.SetActive(false);
            questionButton.SetActive(false);
            //xButon.SetActive(false);
            //questiontext.SetActive(false);
            if (isButtonClicked == true)
            {
                IncreaseTimer();

            }
        }

        if (state == 2)
        {
            instructionPanel.SetActive(false);
            questionButton.SetActive(false);
            //xButon.SetActive(false);
            // questiontext.SetActive(false);
            if (isButtonClicked == true)
            {
                IncreaseTimer();

            }
        }

        if (Input.GetKey(KeyCode.Escape))
        {    
            if (LocalCaseView.isCustomizeButtonClicked)
            {
                this.gameObject.transform.parent.gameObject.SetActive(false);
                localview.SetActive(true);
            }
            else if (LocalCaseView.isLocalCaseDone)
            {
                this.gameObject.transform.parent.gameObject.SetActive(false);
                localview.SetActive(true);
            }
        }  
    }

    public void OnQuestionButtonClick()
    {
        instructionPanel.SetActive(true);
    }

    public void OnXButtonClick()
    {
        if (instructionPanel.activeSelf)
        {
            instructionPanel.SetActive(false);
        }
        else
        {
            xButon.GetComponent<Button>().interactable = false;
        }
    }
}
