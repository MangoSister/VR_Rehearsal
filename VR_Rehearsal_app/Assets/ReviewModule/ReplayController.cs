using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
//Chris Sun
//Control the replay of audio, jumping with slider, play/pause

public class ReplayController : MonoBehaviour {
    private const int FREQUENCY = 44100; //in Hz //CHANGE TO 8000
    private readonly LinkedList<float> recordData = new LinkedList<float>();
    private AudioSource audioSource;
    private PLAY_STATUS playStatus = PLAY_STATUS.STOP;
    private static List<KeyValuePair<float, int>> out_SlidesTransitionRecord;
    private static List<KeyValuePair<float, int>> out_PauseRecord;

    [Header("Playback Slider Control")]
    public Slider playbackSlider;
    public bool preventTrigger = false;
    [Header("Playback Button Control")]
    public Image playButton;
    public Sprite sprPlaying, sprPausing;
    [Header("Time Stamp Control")]
    public Text quarterTime;
    public Text halfTime;
    public Text softTime;
    public Text endTime;
    [Header("Marker Control")]
    public bool isTransitionDisplay = true;
    public bool isPauseDisplay = true;
    public GameObject groupTransitionMarker;
    public GameObject prefabTransitionMarker;
    public GameObject groupPauseMarker;
    public GameObject prefabPauseMarker;
    [Header("Marker Control")]
    public Text testText;

    enum PLAY_STATUS
    {
        STOP = 0,
        PLAY = 1,
        PAUSE = 2
    }

    //public Text t1, t2, t3, t4;

    public void redoRehearsal()
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying == true)
                audioSource.Stop();
        }

        GlobalManager.EnterPresentation();
    }

    public void exitRehearsal()
    {

    }

    private String getTimeString(float time)
    {
        if (time > 60.0f)
            return (int)(time / 60.0f) + ":" + (time - (time / 60) * 60) + "." + (int)((time - (int)(time)) * 100.0f);
        else
            return (int)(time) + "." + (int)((time - (int)(time)) * 100.0f);
    }

    public void jumpInPlayback()
    {
        if (preventTrigger == true)
            return;

        if (audioSource != null)
        {
            if (playbackSlider.value < audioSource.clip.length)
            {
                audioSource.time = playbackSlider.value;

                if (audioSource.isPlaying != true)
                {
                    playButton.sprite = sprPausing;
                    audioSource.Play();
                    playStatus = PLAY_STATUS.PLAY;
                }
            }
        }
        else
            UnityEngine.Debug.Log("cannot find an audiosource");
    }

    public void stopPlaying() //not used.. hmm
    {
        audioSource.Stop();
        audioSource.time = 0;
        playbackSlider.value = 0;
        playStatus = PLAY_STATUS.STOP;
        //playbackButtonText.text = "Play"; //<--- change to change texture instead of text
        playButton.sprite = sprPlaying;
    }

    public void playBack()
    {
        switch (playStatus)
        {
            case PLAY_STATUS.PLAY: //go to pause
                audioSource.Pause();
                playStatus = PLAY_STATUS.PAUSE;
                //playbackButtonText.text = "Play"; ///change texture!
                playButton.sprite = sprPlaying;
                break;
            case PLAY_STATUS.STOP: //go to play, need initializing
            case PLAY_STATUS.PAUSE: //go to play, no need of initializing
                //UnityEngine.Debug.Log("change button text and start playing");
                //playbackButtonText.text = "Pause"; //change texture!
                playButton.sprite = sprPausing;
                audioSource.Play();
                playStatus = PLAY_STATUS.PLAY;
                break;
        }
    }
    
	// Use this for initialization
	void Start () {
        //set up audio source
        if (audioSource == null)
        {
            //UnityEngine.Debug.Log("get audio source"); 
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        if (audioSource.clip != null)
            audioSource.clip = null;

        byte[] byteArray = null;
        if ((PresentationData.out_RecordingFilePath != null) && (PresentationData.out_RecordingFilePath != ""))
            try { byteArray = File.ReadAllBytes(PresentationData.out_RecordingFilePath); }
            catch (FileNotFoundException e) { quarterTime.text=e.Message; }
        else
            byteArray = File.ReadAllBytes(@"C:\Users\xunchis\record.pcm"); //for testing

        //byte > unity float
        if (byteArray == null)
        {
            UnityEngine.Debug.Log("File not found");
            return;
        }

        float[] floatArr = new float[byteArray.Length / 2 + 1];
        int i;
        short max = 0;
        for (i = 0; i < byteArray.Length; i = i + 2)
        {
            //1 short = 2 bytes = 16 bit & in little endian
            byte[] bytebuffer = new byte[2];
            if (BitConverter.IsLittleEndian == true)
            {
                bytebuffer[0] = byteArray[i];
                bytebuffer[1] = byteArray[i + 1];
            }
            else
            {
                bytebuffer[0] = byteArray[i + 1];
                bytebuffer[1] = byteArray[i];
            }

            short valueS = BitConverter.ToInt16(bytebuffer, 0);
            if (valueS > max) max = valueS;

            //translate to -1.0~1.0f
            float valueF = ((float)valueS * 20f) / 32768.0f;
            floatArr[i / 2] = valueF;
        }
        floatArr[i / 2] = '\0';

        //time stamp update
        float totaltime = (floatArr.Length) / 8000.0f;
        quarterTime.text = getTimeString(totaltime / 4.0f);
        halfTime.text = getTimeString(totaltime / 2.0f);
        softTime.text = getTimeString(totaltime * 0.75f);
        endTime.text = getTimeString(totaltime);

        AudioClip myClip = AudioClip.Create("record", floatArr.Length, 1, 8000, false, false);
        myClip.SetData(floatArr, 0);
        audioSource.clip = myClip;
        //UnityEngine.Debug.Log("reset slider"); 
        preventTrigger = true;
        playbackSlider.direction = Slider.Direction.LeftToRight;
        playbackSlider.minValue = 1;
        playbackSlider.maxValue = audioSource.clip.length;
        playbackSlider.value = 0;
        preventTrigger = false;

        if (PresentationData.out_SlidesTransitionRecord != null)
            out_SlidesTransitionRecord = PresentationData.out_SlidesTransitionRecord;

        //for testing
        //out_SlidesTransitionRecord = new List<KeyValuePair<float, int>>();
        //out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(6.0f, 1));
        //out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(10.0f, 1));
        //out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(14.0f, 1));
        //out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(19.0f, 1));
        //out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(22.0f, 1));

        //instantiate markers
        if (isTransitionDisplay == true)
        { 
            foreach (KeyValuePair<float, int> transitionRecord in out_SlidesTransitionRecord)
            {
                var go = Instantiate(prefabTransitionMarker) as GameObject;
                go.transform.parent = groupTransitionMarker.transform;

                //calculate x coord
                float xPos = -54f + (379f - (-54f)) * (transitionRecord.Key / totaltime);

                go.GetComponent<RectTransform>().localPosition = new Vector3(xPos, -68.795f, -110.2937f);
            }
        }

        out_PauseRecord = new List<KeyValuePair<float, int>>();

        if (PresentationData.out_FluencyRecord != null)
        {
            //long pause threshold is set to 3
            int accumulatedTime = 0; //in ms!

            for (int j = 0; j < PresentationData.out_FluencyRecord.Count; j++)
            {
                //check the length of current activity
                int length = PresentationData.out_FluencyRecord[j].Value - accumulatedTime;

                if (accumulatedTime != 0) //ignore the first pause
                {
                    if ((PresentationData.out_FluencyRecord[j].Key.ToString() == "False") && (length > 2000))
                    {
                        out_PauseRecord.Add(new KeyValuePair<float, int>((float)accumulatedTime / 1000.0f, 1));//not speaking
                    }
                }

                accumulatedTime = PresentationData.out_FluencyRecord[j].Value;
            }
        }

        //instantiate markers
        if (isPauseDisplay == true)
        {
            foreach (KeyValuePair<float, int> pauseRecord in out_PauseRecord)
            {
                var go = Instantiate(prefabPauseMarker) as GameObject;
                go.transform.parent = groupPauseMarker.transform;

                //calculate x coord
                float xPos = -101f + (331f - (-101f)) * (pauseRecord.Key / totaltime);

                go.GetComponent<RectTransform>().localPosition = new Vector3(xPos, -63f, -110f);
                go.GetComponent<RectTransform>().localScale = new Vector3(2.0f, 2.0f, 2.0f);
                go.GetComponent<PauseController>().time = pauseRecord.Key;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
        preventTrigger = true;

        if (audioSource != null)
        {
            if (audioSource.isPlaying == true)
            {
                playbackSlider.value = audioSource.time;

                if (playbackSlider.value == playbackSlider.maxValue)
                {
                    audioSource.Stop();
                    audioSource.time = 0;
                    playbackSlider.value = 0;
                    playStatus = PLAY_STATUS.STOP;
                    //playbackButtonText.text = "Play"; <--- change texture

                    playButton.sprite = sprPlaying;
                    playStatus = PLAY_STATUS.STOP;
                }
            }
        }
        preventTrigger = false;
	}
}
