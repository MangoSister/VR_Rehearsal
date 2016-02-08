/*
 * Chris sun, Brandon Kang 
 * update: jan 24, 2016
 * 
 * 
*/
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class micManager : MonoBehaviour {

	private const int FREQUENCY = 12000;

	AudioSource _recordAudioSrc;
	voiceActivityDetector _VAD;

    [SerializeField]
    private bool _isSpeaking;
    public bool isSpeaking { get { return _isSpeaking; } }

    public int recordLengthSec = 30;

	void Start () 
	{
        if (!InitializeMic())
            return;
        _VAD = new voiceActivityDetector(_recordAudioSrc.clip);
    }
	
	void FixedUpdate () 
	{
        _isSpeaking = Microphone.IsRecording(null) && _VAD.CheckActivity();
    }
	// Mic Setup
	bool InitializeMic()
	{

        foreach (string device in Microphone.devices){
			Debug.Log("Name: " + device);
		}
		// @@ microphone.Start(device name, loop, length second, frequency)
		// 1. null means default microphone
		// 2. if we need to record entire rehearsal, it should be changed
		_recordAudioSrc = GetComponent<AudioSource> ();

		int minFreq = 0;
		int maxFreq = 0;
		Microphone.GetDeviceCaps (null, out minFreq, out maxFreq);

		Debug.Log (minFreq + maxFreq);

		_recordAudioSrc.clip = Microphone.Start( null ,true, recordLengthSec, FREQUENCY);

		// Set the AudioClip to loop
		_recordAudioSrc.loop = true; 
		// Mute the sound, we don't want the player to hear it
		_recordAudioSrc.mute = false;

        // @@ ?? Wait until the recording has started
        // if it Keep failing?? 
        while (!(Microphone.GetPosition(null) > 0))
        {

        }
        // Play the audio source!
        _recordAudioSrc.Play();

        return true;
    }
	


}
