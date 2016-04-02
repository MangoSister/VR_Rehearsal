using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomizeView : MonoBehaviour  {

    public static bool isCustomizeDone;
    private SetupManager _setManager;
    public GameObject navi;

    //Set Name of Showcase
    public InputField showCaseTitle;
    public string showCaseName;

    //size of room
    public int roomNumber;

    //percentage of audience
    public Slider sliderVal;
       //set timer
    public InputField timer;
    public string time;

    //local ppt ID
    private string _pptID;
    // Use this for initialization
    bShowcaseManager.showcase_Data customData;
     void Start () {
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        isCustomizeDone = false;
    }

    public void CheckToggle(int index)
    {
        switch (index)
        {
            case 2:
                Debug.Log("Fucking Large");
                customData._mapIdx = (ushort)1;
                break;
            case 1:
                Debug.Log("Fucking Medi");
                customData._mapIdx = (ushort)2;
                break;
            case 0:
                Debug.Log("Fucking small");
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
        isCustomizeDone = true;
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

  
  
}
