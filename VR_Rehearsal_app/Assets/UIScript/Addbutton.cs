using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Addbutton : MonoBehaviour {

	public GameObject uiManager;
	private bool isDown;
	private float downTime;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(!this.isDown)return;
		if(Time.realtimeSinceStartup - this.downTime >2f){
			Debug.Log("Handle Long Tap");
			this.isDown = false;
		}
	}
	public void OnAddButtonClick(){
		uiManager.GetComponent<UIManager>().ShowUrlPanel();
	}
	public void OnPointerDown(PointerEventData eventData){
		this.isDown = true;
		Debug.Log("DOWN!!");
		this.downTime = Time.realtimeSinceStartup;
	}
	
	public void OnPointerUp(PointerEventData eventData){
		this.isDown= false;
		Debug.Log("Up!");
	}


}
