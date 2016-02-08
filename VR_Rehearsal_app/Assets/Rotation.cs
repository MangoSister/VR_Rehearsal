using UnityEngine;
using System.Collections;

public class Rotation : MonoBehaviour {

   
    // Use this for initialization
    void Start () {
	
	}

    // Update is called once per frame
    void Update() {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            // SceneManager.LaunchPresentationScene(new PresentationInitParam("sc_present_0"));
            Application.LoadLevel("sc_present_0");
        }
    }
	
}
