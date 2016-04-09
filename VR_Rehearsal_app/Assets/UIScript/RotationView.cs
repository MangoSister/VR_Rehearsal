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
    public GameObject camera;
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
    // Use this for initialization
    void Start () {
        isRotationDone = false;
        landPanel.SetActive(false);
        verPanel.SetActive(true);
      
	}
	
	// Update is called once per frame
	void Update () {
        if (_isRotate == true)
        {
            IsRotate();
            ChangeLandscapeImage();
        }
    }
    void IsRotate()
    {
        
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            ChangeLandscapeImage();
            PresentationData.in_SlidePath = _localPath;
            PresentationData.in_ExpectedTime = _expectedTime;
            Debug.Log("local Path = "+_localPath);
            Debug.Log("expected Time = " + _expectedTime);
            // the unit of in_ExpectedTime is second
            PresentationData.in_ExpectedTime = _expectedTime * 60;
            switch (_sizeOfRoom)
            {
                case 0:
                    PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                    break;
                case 2:
                    PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                    break;

                case 3:
                    PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                    break;

            }
            // _setManager.BShowcaseMgr.End();
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            PresentationData.in_SlidePath = _localPath;
            PresentationData.in_ExpectedTime = _expectedTime;
            Debug.Log("local Path = " + _localPath);
            Debug.Log("expected Time = " + _expectedTime);
            // the unit of in_ExpectedTime is second
            PresentationData.in_ExpectedTime = _expectedTime * 60;
            switch (_sizeOfRoom)
            {
                case 0:
                    PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                    break;
                case 2:
                    PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                    break;

                case 3:
                    PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                    break;

            }
            ///  _setManager.BShowcaseMgr.End();
            // prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
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
    void ChangeLandscapeImage()
    {
        camera.GetComponent<Camera>().aspect = width / height;
        verPanel.SetActive(false);
        landPanel.SetActive(true);
    }
}
