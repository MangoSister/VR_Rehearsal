using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonType : MonoBehaviour {

    public string buttonName;
   
    public GameObject dfd;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void getType()
    {
        buttonName = GetComponent<RectTransform>().FindChild("pptName").GetComponent<Text>().text;
        string type = GetComponent<RectTransform>().FindChild("Date").GetComponent<Text>().text;

        if (type == "folder")
        {
            GameObject.Find("CanvasManager").GetComponent<UIManager>().UpdateButtons(buttonName);
        }
        else if (type == "file") {
            GameObject.Find("CanvasManager").GetComponent<UIManager>().Download();
        }

    }

    public string returnType()
    {
        return buttonName;
    }

    
}
