using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeSlideText : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    public void UpdateText(string text)
    {
        gameObject.GetComponent<Text>().text = text;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
