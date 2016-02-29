using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	
	bhClowdDriveAPI clowdAPI;
	public GameObject pf;
	string str;
	bool flag;

	// Use this for initialization
	void Start () {
		clowdAPI = new bDropboxAPI ();
		clowdAPI.StartAuthentication ();


		bool res1 = clowdAPI.DonwloadAllFilesInFolder ("/Photos", Application.persistentDataPath , delegate() {
			Debug.Log ("Update Complete");
			clowdAPI.JobDone ();
		});

	}
	
	// Update is called once per frame
	void Update () {
		clowdAPI.Update ();

	}

	void haha(string json){
		Debug.Log (json);
		Debug.Log ("Update Complete");
		clowdAPI.JobDone ();
	}

	void downloadComplete(){
		Debug.Log ("Download Complete");
		clowdAPI.JobDone ();
	}


}
