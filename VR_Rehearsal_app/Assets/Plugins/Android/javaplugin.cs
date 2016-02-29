using UnityEngine;
using System.Collections;




public class javaplugin : MonoBehaviour {

//	public GameObject fff;
//	public TextMesh textM;
	private AndroidJavaObject toastExample = null;
	private AndroidJavaObject activityContext = null;
	string subject = "WORD-O-MAZE";
	string body = "PLAY THIS AWESOME. GET IT ON THE PLAYSTORE";
	// Use this for initialization
	void Start() {
		AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
		currentActivity.Call ("initialize");

	//	textM.text = "It is work";
	}




}
