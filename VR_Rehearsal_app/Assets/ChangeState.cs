using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChangeState : MonoBehaviour {
    public Sprite sprDotOn, sprDotOff;    

	// Use this for initialization
	void Start () {
	
	}
	
    public void SwitchStateTo(int state)
    {
        if (state == 0)
            gameObject.GetComponentInChildren<Image>().sprite = sprDotOff;
        else
            gameObject.GetComponentInChildren<Image>().sprite = sprDotOn;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
