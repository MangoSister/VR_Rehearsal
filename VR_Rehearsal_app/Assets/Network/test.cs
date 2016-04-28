using UnityEngine;
using System.Collections;
using SimpleJSON;

public class test : MonoBehaviour {


	// Use this for initialization
	void Start () {
		AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
		long temp = currentActivity.CallStatic<long> ("GetAvailableMemory", Application.persistentDataPath);
		Debug.Log ("Available Memory Size :" + temp);
	}
	
	// Update is called once per frame
	void Update () {
		//clowdAPI.Update ();

	}



}
