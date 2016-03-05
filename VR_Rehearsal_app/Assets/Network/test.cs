using UnityEngine;
using System.Collections;
using SimpleJSON;

public class test : MonoBehaviour {

	bShowcaseManager showcaseMgr;
	bhClowdDriveAPI clowdAPI;
	public GameObject pf;
	string str;
	bool flag;

	// Use this for initialization
	void Start () {
		
		showcaseMgr = new bShowcaseManager ();
		showcaseMgr.Start ();
		//showcaseMgr.AddShowcase ("jake1", 1, "coll/coll", 30);
		//showcaseMgr.AddShowcase ("jake2", 1, "coll/coll", 30);
		//showcaseMgr.AddShowcase ("jake3", 1, "coll/coll", 30);
		showcaseMgr.End ();
	}
	
	// Update is called once per frame
	void Update () {
		//clowdAPI.Update ();

	}



}
