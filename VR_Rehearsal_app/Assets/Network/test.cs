using UnityEngine;
using System.Collections;
using SimpleJSON;

public class test : MonoBehaviour {
	
	bhClowdDriveAPI clowdAPI;
	public GameObject pf;
	string str;
	bool flag;

	// Use this for initialization
	void Start () {
		clowdAPI = new bLocalDriveAPI ();
		clowdAPI.StartAuthentication ();

		clowdAPI.GetFileListFromPath (Application.persistentDataPath, delegate(string json) {
			Debug.Log (json);
			var parseResult = JSON.Parse(json);

			for (int index = 0; index < parseResult["entries"].Count; index++ )
			{
				Debug.Log(parseResult["entries"][index]["name"].Value);
			}

		
		});

		clowdAPI.DonwloadAllFilesInFolder (Application.persistentDataPath, Application.persistentDataPath + "/babo", delegate() {
			Debug.Log("Done");
		});

		/*
		bool res1 = clowdAPI.DonwloadAllFilesInFolder ("/Photos", Application.persistentDataPath , delegate() {
			Debug.Log ("Update Complete");
			clowdAPI.JobDone ();
		});
		*/

	}
	
	// Update is called once per frame
	void Update () {
		//clowdAPI.Update ();

	}



}
