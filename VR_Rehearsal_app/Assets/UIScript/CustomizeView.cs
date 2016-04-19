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

    //local ppt ID
    private string _pptID;
    // Use this for initialization
    bShowcaseManager.showcase_Data customData;
    public bool defaultValue = false;
    public bool isFromCustom;

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

     void Start () {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetisFromCustom(false);
        isCustomizeDone = false;
      //  isCustomizeDoneFromLocal = false;
        /*Default Value for Customize 
        show case titl = Title
        timer = 5
        room = RPIS
        */

    }
    public void DefaultValueSetting()
    {
        showCaseTitle.GetComponent<InputField>().text = _defaultTitle;
        customData._showcaseName = _defaultTitle;
        timer.GetComponent<InputField>().text = _defaultTime;
        tgroup.GetComponent<RectTransform>().FindChild("big").GetComponent<Toggle>().isOn = true;

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
  
    public void CheckSliderValue()
    {
        customData._percentageOfAudience =(ushort) sliderVal.value; 
    }

    public void SetTimer()
    {
        customData._expetedTime_min = (ushort)(int.Parse(timer.text));
    }

    public void SetShowCaseName()
    {
        customData._showcaseName = showCaseTitle.text;
    }
    public void CustomCompleteClicked()
    {
        _setManager.BShowcaseMgr.EditShowcase(_pptID, customData._showcaseName, customData._mapIdx, Application.persistentDataPath + "/" + _pptID, customData._percentageOfAudience, customData._expetedTime_min);
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
        rotationView.GetComponent<RotationView>().SetData(customData._showcaseName, customData._mapIdx, customData._percentageOfAudience, Application.persistentDataPath + "/" + _pptID, _pptID, customData._expetedTime_min);
        isCustomizeDone = true; //this will trigger the scene to move forward to calibration
        GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetisFromCustom(true);
        Debug.Log(customData._mapIdx);
        gameObject.SetActive(false);
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
}
