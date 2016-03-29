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
    public Slider playbackSlider;
    private AudioSource audioSource;
    public bool preventTrigger = false;
    private PLAY_STATUS playStatus = PLAY_STATUS.STOP;
    public Image playButton;
    public Sprite sprPlaying, sprPausing;

    enum PLAY_STATUS
    {
        STOP = 0,
        PLAY = 1,
        PAUSE = 2
    }

    //public Text t1, t2, t3, t4;

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
        if (audioSource == null)
        {
            //UnityEngine.Debug.Log("get audio source"); 
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        if (audioSource.clip != null)
            audioSource.clip = null;

        //HARDCODED =====================================================================
        byte[] byteArray = File.ReadAllBytes(@"C:\Users\xunchis\record.pcm");

        //byte > unity float
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

        /*
            float totaltime = (floatArr.Length) / 8000.0f;
            int quarter = (int)(totaltime / 4.0f);

            if (quarter > 60)
            {
                t1.text = "0:0.00";
                t2.text = quarter / 60 + ":" + (quarter - (quarter / 60) * 60) + "." + (int)((((float)totaltime) / 4.0f - quarter) * 100.0f);
            }
            else
            {
                t1.text = "0.00";
                t2.text = quarter + "." + (int)((((float)totaltime) / 4.0f - quarter) * 100.0f);
            }

            int soft = quarter * 3;
            if  (soft>60)
                t3.text = soft / 60 + ":" + (soft - (soft / 60) * 60) + "." + (int)((((float)totaltime) * 0.75f - soft) * 100.0f);
            else
                t3.text = soft + "." + (int)((((float)totaltime) * 0.75f - soft) * 100.0f);

            if (totaltime > 60)
                t4.text = totaltime / 60 + ":" + (totaltime - (totaltime / 60) * 60) + "." + (int)((((float)totaltime) - (int)totaltime) * 100.0f);
            else
                t4.text = totaltime.ToString("0.00");
        */

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
