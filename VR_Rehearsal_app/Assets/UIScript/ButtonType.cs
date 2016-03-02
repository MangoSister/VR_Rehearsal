using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonType : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{

   // public string buttonName;
    public string buttonName;
    public string buttonType;
    public bool isSelected = false;
    public bool isReleased = false;

	// Use this for initialization
	void Start () {
       
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    
    public void GetButtonStatus()
    {
        if (isSelected == false && isReleased == false)
        {
            isSelected = true;
            Debug.Log("Clicked");
        }

    }
    public void GetButtonRealse()
    {
        if (isSelected == true && isReleased == false)
        {
            isSelected = false;
            isReleased = true;
            Debug.Log("Releasd!");
        }
        isReleased = false;

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
    public void OnPointerDown(PointerEventData eventData)
    {

        if (isSelected == false && isReleased == false)
        {
            isSelected = true;
        }
       
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(isSelected == true && isReleased ==false)
        {
            isSelected = false;
            isReleased = true;
        }
        isReleased = false;
       
    }

}
