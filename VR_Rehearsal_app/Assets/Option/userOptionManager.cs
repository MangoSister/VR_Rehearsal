using UnityEngine;
using System.Collections;

/*
 *  Save user preference data;
 */
public class userOptionManager : MonoBehaviour {
	
	//- Option Setting for voice echo Effect 
	private float _voiceEchoEffect_Volume;
	public float voiceEchoEffect_Volume{
		get{ 
			PlayerPrefs.GetFloat("option_voiceEchoVolume");
			return _voiceEchoEffect_Volume;
		}
		set{ 
			_voiceEchoEffect_Volume = Mathf.Clamp(value,0f,1f);
			PlayerPrefs.SetFloat("option_voiceEchoVolume", _voiceEchoEffect_Volume);
		}
	}

	//- Option Setting for voice echo Effect 
	private bool  _crosshairToggle;
	public bool crosshairToggle{
		get{ 
			_tutorialToggle = System.Convert.ToBoolean(PlayerPrefs.GetInt ("option_crosshairToggle"));
			return _crosshairToggle;
		}
		set{ 
			_crosshairToggle = value;
			PlayerPrefs.SetInt ("option_crosshairToggle", System.Convert.ToInt32(_crosshairToggle));
		}
	}

	//- Option setting for tutorial ON/OFF
	private bool _tutorialToggle;
	public bool tutorialToggle{
		get{ 
			_tutorialToggle = System.Convert.ToBoolean(PlayerPrefs.GetInt ("option_tutorialToggle"));
			return _tutorialToggle;
		}
		set { 
			_tutorialToggle = value;
			PlayerPrefs.SetInt ("option_tutorialToggle", System.Convert.ToInt32(_tutorialToggle));
		}
	}



}
