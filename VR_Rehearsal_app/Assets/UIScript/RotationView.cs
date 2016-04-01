using UnityEngine;
using System.Collections;

public class RotationView : MonoBehaviour {
    static public bool isRotationDone;
    public GameObject prepHouse;
    private SetupManager _setManager;
    public bool _isRotate = false;
    // Use this for initialization
    void Start () {
        isRotationDone = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (_isRotate == true)
        {
            IsRotate();
        }
    }
    void IsRotate()
    {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
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
}
