using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomizeView : MonoBehaviour  {
    public static bool isCustomizeDone;
    public bool isCustomizeDoneFromLocal;
    private SetupManager _setManager;
    public GameObject rotationView;
    public GameObject navi;
    public GameObject warningText;

    //Set Name of Showcase
    public InputField showCaseTitle;
    public string showCaseName;
    private string _defaultTitle = "Title";

    //size of room
    public GameObject tgroup;
    public int roomNumber;
    private int _defaultRoom = 1;

    //percentage of audience
    public Slider sliderVal;
       //set timer
    public InputField timer;
    public string time;
    private string _defaultTime = "5";

    // Check Ech0
    public Toggle isEcho;
    //local ppt ID
    private string _pptID;
    // Use this for initialization
    bShowcaseManager.showcase_Data customData;
    public bool defaultValue = false;
    public bool isFromCustom;

    //checkMemory Warnging Sign
    public GameObject memoryWarningPanel;
    bool enoughMemory;
    
    [Header("Venue Select")]
    public Button btnRoom1;
    public Button btnRoom2;
    public Button btnRoom3;
    public Sprite room1Checked;
    public Sprite room1Unchecked;
    public Sprite room2Checked;
    public Sprite room2Unchecked;
    public Sprite room3Checked;
    public Sprite room3Unchecked;
	AndroidJavaObject _currentActivity;

     void Start () {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        warningText.SetActive(false);
        GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetisFromCustom(false);
        isCustomizeDone = false;
        enoughMemory = true;
        #if UNITY_EDITOR


#elif UNITY_ANDROID
			AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			_currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
#endif
    }
    public void DefaultValueSetting()
    {
        showCaseTitle.GetComponent<InputField>().text = _defaultTitle;
        customData._showcaseName = _defaultTitle;
        timer.GetComponent<InputField>().text = _defaultTime;
        tgroup.GetComponent<RectTransform>().FindChild("big").GetComponent<Toggle>().isOn = true;
        //echo Toggle.
        customData._expetedTime_min = (ushort)(int.Parse(_defaultTime));
    }

    public void CheckToggle(int index)
    {
        switch (index)
        {
            case 2:
                btnRoom3.GetComponent<Image>().sprite = room3Checked;
                btnRoom1.GetComponent<Image>().sprite = room1Unchecked;
                btnRoom2.GetComponent<Image>().sprite = room2Unchecked;
                customData._mapIdx = (ushort)1;
                break;
            case 1:
                btnRoom3.GetComponent<Image>().sprite = room3Unchecked;
                btnRoom1.GetComponent<Image>().sprite = room1Unchecked;
                btnRoom2.GetComponent<Image>().sprite = room2Checked;
                customData._mapIdx = (ushort)2;
                break;
            case 0:
                btnRoom3.GetComponent<Image>().sprite = room3Unchecked;
                btnRoom1.GetComponent<Image>().sprite = room1Checked;
                btnRoom2.GetComponent<Image>().sprite = room2Unchecked;
                customData._mapIdx = (ushort)3;
                break;
            default:
                customData._mapIdx = (ushort)0;
                break;
        }
    }
    public void CheckEcho()
    {
        if (isEcho.GetComponent<Toggle>().isOn)
        {
            PresentationData.in_VoiceEcho = true;
            customData._isEchoEffect = true;
        }
        else
        {
            PresentationData.in_VoiceEcho = false;
            customData._isEchoEffect = false;
        }
    }
    public void CheckSliderValue()
    {
        customData._percentageOfAudience =(ushort) sliderVal.value; 
    }

    public void SetTimer()
    {
        ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        if (timer.text == "")
        {
            customData._expetedTime_min = (ushort)(int.Parse("0")); ;
        }
        else {
        #if UNITY_EDITOR
            customData._expetedTime_min = (ushort)(int.Parse(timer.text));
#elif UNITY_ANDROID
            /* This is for checking memory availablity*/
            bool res = CheckAvailableMemory ();
			if (res) { /*Enough free memory*/
				customData._expetedTime_min = (ushort)(int.Parse (timer.text));
                enoughMemory = true;
			}
           
            else { /*No enough Memory */
                     /*code below from here*/
                     enoughMemory = false;
                    memoryWarningPanel.SetActive(true);
                    
			 }              
#endif


        }
    }
    IEnumerator WarningSign()
    {
        warningText.SetActive(true);
        yield return new WaitForSeconds(2);
        warningText.SetActive(false);
    }
    public void SetShowCaseName()
    {
        customData._showcaseName = showCaseTitle.text;
    }
    public void CustomCompleteClicked()
    {
        if (customData._expetedTime_min < 20 && enoughMemory == true)
        {
            _setManager.BShowcaseMgr.EditShowcase(_pptID, customData._showcaseName, customData._mapIdx, Application.persistentDataPath + "/" + _pptID, customData._percentageOfAudience, customData._expetedTime_min, customData._isEchoEffect);
            if (navi.GetComponent<NavigationView>().storedButton.Count > 0)
            {
                foreach (RectTransform child in navi.GetComponent<NavigationView>().contentRect)
                {
                    if (child.name == "PPT_Practice(Clone)")
                    {
                        GameObject.Destroy(child.gameObject);
                    }
                }
                navi.GetComponent<NavigationView>().storedButton.Clear();
            }
            rotationView.GetComponent<RotationView>().SetData(customData._showcaseName, customData._mapIdx, customData._percentageOfAudience, Application.persistentDataPath + "/" + _pptID, _pptID, customData._expetedTime_min, customData._isEchoEffect);
            isCustomizeDone = true; //this will trigger the scene to move forward to calibration
            GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetisFromCustom(true);
            Debug.Log(customData._mapIdx);
            gameObject.SetActive(false);
        }
      
        else if(customData._expetedTime_min >20)
        {
            StartCoroutine("WarningSign");
        }
    }
    public void WarningTime()
    {
        if (timer.text == "")
        {
            int _timer = 0;
        }
        else if(int.Parse(timer.text) > 20)
        {
            StartCoroutine("WarningSign");
        }
    }
    public void SetPPTID(string id)
    {
        _pptID = id;
    }

    public void SetSetupManager(SetupManager mg)
    {
        _setManager = mg;
    }

    public void SetCustomValueFromLocalView(string oldTitle, int oldSizeOfRoom, int oldAudience, string oldLocalPath, int oldTime)
    {
        customData._showcaseName = oldTitle;
        customData._mapIdx = (ushort)oldSizeOfRoom;
        customData._percentageOfAudience = (ushort)oldAudience;
        customData._pptFolderPath = oldLocalPath;
        customData._expetedTime_min = (ushort)oldTime;
        showCaseTitle.GetComponent<InputField>().text = oldTitle;
        if (isEcho.GetComponent<Toggle>().isOn)
        {
            PresentationData.in_VoiceEcho = true;
            customData._isEchoEffect = true;
        }
        else
        {
            PresentationData.in_VoiceEcho = false;
            customData._isEchoEffect = false;
        }
        switch (oldSizeOfRoom)
        {
            case 3:
                tgroup.GetComponent<RectTransform>().FindChild("SmallRoom").GetComponent<Toggle>().isOn = true;
                break;
            case 2:
                tgroup.GetComponent<RectTransform>().FindChild("Medium ").GetComponent<Toggle>().isOn = true;
                break;
            case 1:
                tgroup.GetComponent<RectTransform>().FindChild("big").GetComponent<Toggle>().isOn = true;
                break;
            default:
                tgroup.GetComponent<RectTransform>().FindChild("big").GetComponent<Toggle>().isOn = false;
                tgroup.GetComponent<RectTransform>().FindChild("Medium ").GetComponent<Toggle>().isOn = false;
                tgroup.GetComponent<RectTransform>().FindChild("SmallRoom").GetComponent<Toggle>().isOn = false;
                break;
        }
        timer.GetComponent<RectTransform>().FindChild("Text").GetComponent<Text>().text = oldTime.ToString();
        timer.GetComponent<InputField>().text = oldTime.ToString();
        isCustomizeDoneFromLocal = true;
        defaultValue = false;
    }

	public bool  CheckAvailableMemory(){
		long currentAvailableMemorySize = _currentActivity.CallStatic<long> ("GetAvailableMemory", Application.persistentDataPath);
		int resTime = customData._expetedTime_min + 10; 
		if ((resTime * 5400000) > currentAvailableMemorySize) {
			return false;
		}
		return true;
	}

    public void WarningOkButtonClick()
    {
        memoryWarningPanel.SetActive(false);
    }



}
