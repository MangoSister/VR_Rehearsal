using UnityEngine;
using System.Collections;

public class clockTimer : MonoBehaviour {


	public TextMesh textMesh;
	public Transform timerSpear;
	public float _settedSecond = 0;
	private float _currSecond = 0f; 

	// Use this for initialization
	void Start () {
		_currSecond = _settedSecond;
	}

	public void SetTimer(float time){
		_settedSecond = time;
		_currSecond = _settedSecond;
	}


	// Update is called once per frame
	void Update () {
		 
		if (_currSecond > 0) {
			_currSecond -= Time.deltaTime;

			int minutes = (int)_currSecond/60;
			int seconds = (int)_currSecond%60;

			if(minutes == 0 && seconds < 10.0){
				textMesh.color= Color.red;
			}

			textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
			timerSpear.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0,360, (_settedSecond - _currSecond)/_settedSecond  ));
		
		} else {
			textMesh.color= Color.black;

		}
	
	}




}
