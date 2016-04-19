using UnityEngine;
using System.Collections;

public class RotationView : MonoBehaviour {
    static public bool isRotationDone;
    public GameObject prepHouse;
    public bool _isRotate = false;
    public GameObject verPanel;
    public GameObject landPanel;
    private SetupManager _setManager;
    public float width = 16;
    public float height = 9f;
    bShowcaseManager.showcase_Data customData;
    private string _showCaseName;
    private int _sizeOfRoom;
    private int _numberOfAudience;
    private string _localPath;
    private string _id;
    private int _expectedTime;

    public void SetData(string showCanseName, int sizeOfRoom, int numberOfAudience, string localPath, string id, int time)
    {
        _showCaseName = showCanseName;
        _sizeOfRoom = sizeOfRoom;
        _numberOfAudience = numberOfAudience;
        _localPath = localPath;
        _id = id;
        _expectedTime = time;
    }
    void Start () {
        isRotationDone = false;
        verPanel.SetActive(true);
	}
	
	void Update () {
        if (_isRotate == true)
        {
            IsRotate();
        }
    }
    void IsRotate()
    {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
        {
            ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
            if (CanvasManager.againTrigger == true)
            {
                GlobalManager.EnterPresentation();
                CanvasManager.againTrigger = false;
            }
            else if (GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetisFromCustom())
            {
                PresentationData.in_SlidePath = _localPath;
                PresentationData.in_ExpectedTime = _expectedTime * 60;
                switch (_sizeOfRoom)
                {
                    case 1:
                        PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                        break;
                    case 2:
                        PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                        break;

                    case 3:
                        PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                        break;
                }
            }
            else
            {
                PresentationData.in_SlidePath = GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetLocalPath();
                PresentationData.in_ExpectedTime = GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetTime() * 60;
                _sizeOfRoom = GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetRoom();
                switch (_sizeOfRoom)
                {
                    case 1:
                        PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                        break;
                    case 2:
                        PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                        break;

                    case 3:
                        PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                        break;
                }
            }

            GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetisFromCustom(false);
            GlobalManager.EnterPresentation();
        }

        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (CanvasManager.againTrigger)
            {
                GlobalManager.EnterPresentation();
                CanvasManager.againTrigger = false;
            }
            else if (GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetisFromCustom())
            {
                //pc local custombutton-> custom -> rotation -> scene;
                PresentationData.in_SlidePath = _localPath;
                PresentationData.in_ExpectedTime = _expectedTime * 60;
                switch (_sizeOfRoom)
                {
                    case 1:
                        PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                        break;
                    case 2:
                        PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                        break;

                    case 3:
                        PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                        break;

                }
            }
            else if (CanvasManager.againTrigger)
            {
                string id = PlayerPrefs.GetString("currentShowcase");
                Debug.Log("TEST --=== " + id);
                if (id != null)
                {
                    bShowcaseManager.showcase_Data? tempShowCaseData = _setManager.BShowcaseMgr.GetSignleShowcase(id);
                    PresentationData.in_SlidePath = tempShowCaseData.Value._pptFolderPath;
                    PresentationData.in_ExpectedTime = (int)tempShowCaseData.Value._expetedTime_min;
                    _sizeOfRoom = (int)tempShowCaseData.Value._mapIdx;
                    switch (_sizeOfRoom)
                    {
                        case 1:
                            PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                            break;
                        case 2:
                            PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                            break;

                        case 3:
                            PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                            break;
                    }
                }
            }
            else
            //pc -> custom -> rotation -> scene;
            {
                PresentationData.in_SlidePath = GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetLocalPath();
                PresentationData.in_ExpectedTime = GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetTime() * 60;
                _sizeOfRoom = GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().GetRoom();
                switch (_sizeOfRoom)
                {
                    case 1:
                        PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                        break;
                    case 2:
                        PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                        break;

                    case 3:
                        PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                        break;
                }
            }
        
            GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetisFromCustom(false);
            GlobalManager.EnterPresentation();
        }
    }
    public void SetSetupManager(SetupManager mg)
    {
        _setManager = mg;
    }
    public void SetRotation(bool rotation)
    {
        _isRotate = rotation;
    }
 }
