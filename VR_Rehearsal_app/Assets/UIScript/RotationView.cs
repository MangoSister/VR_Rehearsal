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
           // _setManager.BShowcaseMgr.End();
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
          ///  _setManager.BShowcaseMgr.End();
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
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
