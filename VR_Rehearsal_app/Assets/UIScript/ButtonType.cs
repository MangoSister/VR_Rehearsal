using UnityEngine;
using System.Collections;
using UnityEngine.UI;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class ButtonType : MonoBehaviour// UIBehaviour,IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{

   // public string buttonName;
    public string buttonName;
    public string buttonType;
    public bool isSelected = false;
    public bool isReleased = false;

    public float durationThreshold = 0.5f;

   // public UnityEvent onLongPress = new UnityEvent();

    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float timePressStarted;

    private void Update()
    {/*
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
            }
            else if (Time.time - timePressStarted < durationThreshold)
            {

            }
         }
         */
    }

    public void GetButtonStatus()
    {
        if (isSelected == false && isReleased == false)
        {
            isSelected = true;
			#if UNITY_EDITOR
            Debug.Log("Clicked");
			#endif
        }
    }
    public void GetButtonRealse()
    {
        if (isSelected == true && isReleased == false)
        {
            isSelected = false;
            isReleased = true;
			#if UNITY_EDITOR
            Debug.Log("Releasd!");
			#endif
        }
        isReleased = false;
    }
    /*
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        timePressStarted = Time.time;
        if (Time.time - timePressStarted < durationThreshold/2f) { 
            if (isSelected == false && isReleased == false)
            {
               isPointerDown = true;
               longPressTriggered = false;
               isSelected = true;
                Debug.Log("PointDown");
            }
         }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(isSelected == true && isReleased ==false)
        {
            isPointerDown = false;
            isSelected = false;
            isReleased = true;
            Debug.Log("PointUp");
        }
        isReleased = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
    }
    */
}
