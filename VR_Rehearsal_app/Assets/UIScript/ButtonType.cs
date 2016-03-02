using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonType : MonoBehaviour {

   // public string buttonName;
   
   public RectTransform rootRect;
   public bool isSelection = false;

   public string buttonName;
   public string buttonType;
   public bool isSelected = false;

	// Use this for initialization
	void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void GetButtonStatus()
    {
       
        if(isSelected == false)
        {
            isSelected = true;
        }
    }
    
}
