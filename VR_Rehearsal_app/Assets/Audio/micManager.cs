/*
 * Chris sun, Brandon Kang, Yang Zhou
 * last update: Feb 15, 2016
 * 
 * 
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class micManager : MonoBehaviour {

	private const int FREQUENCY = 12000;

	AudioSource _recordAudioSrc;
	voiceActivityDetector _VAD;

	public GameObject text;
	private Text textUI;

    [SerializeField]
    private bool _isSpeaking;
    public bool isSpeaking { get { return _isSpeaking; } }

    public int recordLengthSec = 30;
	private AudioObj[] _audioObj = new AudioObj[2];



	//--------------------------
	private int _index = 0;

	private struct AudioObj
	{
		public AudioClip clip;

		public void SetClip(AudioClip c)
		{
			clip = c;
			/*
     slowing the playback down a small amount allows enough space between
     recording and output so that analysis does not overtake the recording.
     this helps with stutter and distortion, but doesn't solve it completely
     */
		}

	}
	//------------------------------------



	void Start () 
	{
        //Device connection check by Yang 2.15.2016
        if (Microphone.devices.Length <= 0)
        {
            print("No Microphone connected");
            return;
        }

		/*
        if (!InitializeMic())
            return;

        */
		//textUI = text.GetComponent<Text> ();
		for (int i = 0; i < 2; i++) {
			_audioObj[i].clip = new AudioClip();
		}


		_recordAudioSrc = GetComponent<AudioSource> ();
		StartCoroutine(StartRecord());

    }
	
	void FixedUpdate () 
	{
        //_isSpeaking = Microphone.IsRecording(null) && 
	
    }
	// Mic Setup
	bool InitializeMic()
	{

        foreach (string device in Microphone.devices){
#if UNITY_EDITOR
            Debug.Log("Name: " + device);
#endif
        }
		// @@ microphone.Start(device name, loop, length second, frequency)
		// 1. null means default microphone
		// 2. if we need to record entire rehearsal, it should be changed
		_recordAudioSrc = GetComponent<AudioSource> ();

		int minFreq = 0;
		int maxFreq = 0;
		Microphone.GetDeviceCaps (null, out minFreq, out maxFreq);
#if UNITY_EDITOR
        Debug.Log (minFreq + maxFreq);
#endif
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
	
	private IEnumerator StartRecord()
	{

		if ( Microphone.IsRecording(null)) {
			_recordAudioSrc.Stop ();
			Microphone.End(null);
		}

        _audioObj[_index].clip = Microphone.Start(null, true, recordLengthSec, FREQUENCY);
		_VAD = new voiceActivityDetector(	_audioObj[_index].clip);
		/*
     the longer the mic recording time, the less often there are "hiccups" in game performance
     but also due to being pitched down, the playback gradually falls farther behind the recording
     */


		print("recording to audioObj " + _index);
		StartCoroutine(StartPlay());
		yield return new WaitForSeconds(recordLengthSec);
		StartCoroutine(StartRecord()); //swaps audio buffers, begins recording and playback of new buffer
		/* it is necessary to swap buffers, otherwise the audioclip quickly becomes too large and begins to slow down the system */

	}

	private IEnumerator StartPlay()
	{
		//_audioObj[_index].SetClip(buffer);
		yield return new WaitForSeconds(.001f);
		//_audioObj[_index].player.SetActive(true);
		//_audioObj[_index].clip.Play();
		_recordAudioSrc.clip = _audioObj [_index].clip;

		// Set the AudioClip to loop
		_recordAudioSrc.loop = true; 
		// Mute the sound, we don't want the player to hear it
		_recordAudioSrc.mute = false;
		
		// @@ ?? Wait until the recording has started
		// if it Keep failing?? 
		while (!(Microphone.GetPosition(null) > 0))
		{
			
		}

		_recordAudioSrc.Play ();


		StartCoroutine (CheckVAD (_index));
        //_audioObj[Mathf.Abs((_index % 2) - 1)].player.audio.Stop();


       _index = (++_index) % 2;	
       
	}

	private IEnumerator CheckVAD(int idx){

		float currTime = 0;
		while(currTime < 30)
		{
			currTime += Time.deltaTime;
			bool ore = _VAD.CheckActivity(_audioObj [idx].clip);
#if UNITY_EDITOR
            Debug.Log(ore);
#endif



            yield return null;
		}
	}

}
