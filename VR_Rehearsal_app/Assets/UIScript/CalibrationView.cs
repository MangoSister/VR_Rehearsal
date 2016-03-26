using UnityEngine;
using System.Collections;

public class CalibrationView : MonoBehaviour {

    public static bool isCalibrationDone;
	// Use this for initialization
	void Start () {
        isCalibrationDone = false;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DoneButtonClicked()
    {
        isCalibrationDone = true;
        gameObject.SetActive(false);
    }
}
