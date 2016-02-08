using UnityEngine;
using System.Collections;

public class clockTimer : MonoBehaviour {

	public float maxSecond;
	private float _currTime;
	public TextMesh textMesh;


	// Use this for initialization
	void Start () {
		maxSecond = 20;
	}

	void SetTime_Countdown(int maxSec){
		maxSecond = maxSec;
	}


	void Initialize(){


	}


	// Update is called once per frame
	void Update () {
	
		if (maxSecond > 0) {
			maxSecond -= Time.deltaTime;

			int minutes = (int)maxSecond/60;
			int seconds = (int)maxSecond%60;

			if(minutes == 0 && seconds < 30.0){
				textMesh.color= Color.red;
			}

			textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
		
		} else {

		}


	




	}
}
