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
		bool res1 = clowdAPI.GetFileListFromPath ("/", delegate(string resJson){
				Debug.Log (resJson);
				Debug.Log ("Update Complete");
				clowdAPI.JobDone ();

				bool res2 = clowdAPI.GetSelectedFolderFileList("public", delegate(string resJsonh) {
					Debug.Log (resJsonh);
					clowdAPI.JobDone ();

					bool res3 = clowdAPI.GetCurrParentFileList(delegate(string resJJsonh) {
							Debug.Log (resJJsonh);
							clowdAPI.JobDone ();
						});
					Debug.Log(res3);

				}); 
				Debug.Log(res2);
			});

		Debug.Log(res1);
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
