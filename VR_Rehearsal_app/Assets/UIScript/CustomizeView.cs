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
    public Slider sliderVla;
    public float sliderVal;

    //set timer
    public InputField timer;
    public string time;

    //local ppt ID
    private string _pptID;
    // Use this for initialization

    public struct CustomData
    {
        public string title;
        public int sizeRoom, timer;
        public float audience;
 
        public CustomData(string _title,int _sizeRoom, float _audience, int _timer)
        {
            title = _title;
            sizeRoom = _sizeRoom;
            audience = _audience;
            timer = _timer;
        }
    }

    CustomData cData = new CustomData("",0,0f,0);

    void Start () {
        isCustomizeDone = false;
    }

    public void CheckToggle(int index)
    {
        switch (index)
        {
            case 0:
                Debug.Log("Fucking Large");
                cData.sizeRoom = 0;
                break;
            case 1:
                Debug.Log("Fucking Medi");
                cData.sizeRoom = 1;
                break;
            case 2:
                Debug.Log("Fucking small");
                cData.sizeRoom = 2;
                break;
            default:
                cData.sizeRoom = -1;
                break;
        }
    }

    public void CheckSliderValue()
    {
        cData.audience = sliderVla.value;
    }

    public void SetTimer()
    {
        cData.timer = int.Parse(timer.text);
        Debug.Log("TIMER : " + time);
    }

    public void SetShowCaseName()
    {
        cData.title = showCaseTitle.text;
        Debug.Log("ShowCase name : " + showCaseName);
    }

    public void CustomCompleteClicked()
    {
        Debug.Log("PHAN!!");
        _setManager.BShowcaseMgr.EditShowcase(_pptID, showCaseName, 0, Application.persistentDataPath + "/" + _pptID, (int)sliderVal, 5);
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
