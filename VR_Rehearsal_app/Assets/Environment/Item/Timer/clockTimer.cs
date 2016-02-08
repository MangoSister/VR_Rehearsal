using UnityEngine;
using System.Collections;

public class clockTimer : MonoBehaviour {


	public TextMesh textMesh;
	public float maxSecond = 60f; 

	// Use this for initialization
	void Start () {
	
	}


	// Update is called once per frame
	void Update () {
	
		if (maxSecond > 0) {
			maxSecond -= Time.deltaTime;

			int minutes = (int)maxSecond/60;
			int seconds = (int)maxSecond%60;

			if(minutes == 0 && seconds < 10.0){
				textMesh.color= Color.red;
			}

			textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
		
		} else {
			textMesh.color= Color.black;
			maxSecond = 60f;
		}
	
	}




}
