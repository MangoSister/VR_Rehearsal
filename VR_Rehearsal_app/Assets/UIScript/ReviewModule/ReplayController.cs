﻿using UnityEngine;
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
    private List<KeyValuePair<float, int>> out_SlidesTransitionRecord;
    private List<KeyValuePair<int, float>> slideTimingRecord; 
    private List<KeyValuePair<float, int>> out_PauseRecord;
    private bool isProcessingAudio = false; //for the threaded job audio processing
    private AudioProcessingJob pcmToUnityClip;
    private float[] floatArray;
    private float[] floatWaveData; //for pooling wave data, optimization
    private float floatArrayMaximum = 0.0f;
    private int arrayLength = 0;
    private const int CHART_INTERVAL = 30; //30s
    //private float chartStartTime = -CHART_INTERVAL - 1.0f; //for the wave pattern to display at the beginning
    private float slideStartTime = -1.0f, slideEndTime = -1.0f; //for the slide thumbnails group to update at the beginning
    private float frameStartTime = -1.0f, frameEndTime = -1.0f; //for the current slide thumbnails to update at the beginning
    private int nowGroupNo = 0;
    private int startSlideIndex = 0;
    private float lastAudioSourceTime = -1f;
    


    //for volume control
    private AndroidJavaClass unity;
    private AndroidJavaObject currentActivity;

    [Header("Heatmap Generation")]
    private HeatmapGenerator heatMapGen;
    public GameObject heatmapHolder;
    public GameObject screenshotHolder;
    
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

    [Header("For Debugging")]
    public Text testText;
    [Header("Loading")]
    public GameObject loadingText;
    public GameObject groupReplayObjects;
    public GameObject loadingGroup;
    public Image loadingBar;

    [Header("Updating Markers in Zoom-In Of AudioClip")]
    public GameObject currentPositionMarker;
    public GameObject groupOfPauseMarkers;
    public GameObject groupOfTransitionMarkers;
    public GameObject groupOfWaves;
    public GameObject prefabPauseArea;
    public GameObject prefabEndArea;
    public GameObject prefabWave;
    public GameObject prefabTransitionMarker;
    private Sprite[] slidesTexture;

    [Header("Panel Control")]
    public Button btnChangeToHeatmap;
    public Button btnChangeToSpeech;
    public Sprite heatmapOn;
    public Sprite heatmapOff;
    public Sprite speechOn;
    public Sprite speechOff;
    public GameObject panelHeatmap;
    public GameObject panelSpeech;

    [Header("Slide Thumbnails")]
    public GameObject[] slideThumbnails;
    private float[] startTimeForThumbnailGroup;
    public GameObject PageDotGroup;
    public GameObject pfbPageDot;
    public Image currentSlide;
    public Text currentSlideStartTime, currentSlideEndTime;

    [Header("Mike Christel\'s comment")]
    public GameObject ExitButtonGroup;

    //for optimization!
    private GameObject[] waves;
    private GameObject[] pauses;

    enum PLAY_STATUS
    {
        STOP = 0,
        PLAY = 1,
        PAUSE = 2
    }

    public void SwitchToHeatMap()
    {
        btnChangeToHeatmap.image.sprite = heatmapOn;
        btnChangeToSpeech.image.sprite = speechOff;
        panelHeatmap.SetActive(true);
        panelSpeech.SetActive(false);

        //stop playing

        if (playStatus == PLAY_STATUS.PLAY)
            playBack();
    }

    public void SwitchToSpeech()
    {
        btnChangeToHeatmap.image.sprite = heatmapOff;
        btnChangeToSpeech.image.sprite = speechOn;
        panelHeatmap.SetActive(false);
        panelSpeech.SetActive(true);
        ExitButtonGroup.SetActive(true);
    }

    //public Text t1, t2, t3, t4;

    public void redoRehearsal()
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying == true)
                audioSource.Stop();
        }

       // PlayerPrefs.SetString("currentShowcase", _id);
        CanvasManager.againTrigger = true;
        Application.LoadLevel("sc_UI");
         //GlobalManager.EnterPresentation();
    }

    public void exitRehearsal()
    {
        if (audioSource != null)
        {
            if (audioSource.isPlaying == true)
                audioSource.Stop();
        }
        CanvasManager.finishTrigger = true;
        Application.LoadLevel("sc_UI");

    }

    private String getTimeString(float time)
    {
        string timestring = "";

        //get minute
        //if (time >= 600.0f)
            //timestring = (int)(time / 60.0f) + ":" ;
        //    timestring = (int)(time / 60.0f) + "\'";
        //else if (time > 60.0f)
        if (time > 60.0f)
            //timestring = "0" + (int)(time / 60.0f) + ":";
            //timestring = "0" + (int)(time / 60.0f) + "\'";
            timestring = (int)(time / 60.0f) + "\'";
        //else
        //    timestring = "00:";

        //get second
        //if (((int)(time) % 60) >= 10)
            timestring += ((int)(time) % 60).ToString()+"\"";// + "." + (int)((time - (int)(time)) * 100.0f);
        //else
        //    timestring += "0" + ((int)(time) % 60) + "\"";// + "." + (int)((time - (int)(time)) * 100.0f);

        return timestring;
    }

    private String getTimeString2(float time)
    {
        string timestring = "";

        //get minute
        if (time >= 600.0f)
              timestring = (int)(time / 60.0f) + ":" ;
        else if (time > 60.0f)
            timestring = "0" + (int)(time / 60.0f) + ":";
        else
            timestring = "00:";

        //get second
        if (((int)(time) % 60) >= 10)
            timestring += ((int)(time) % 60).ToString();
        else
            timestring += "0" + ((int)(time) % 60);

        return timestring;
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

    public void RefreshTopChart()
    {
        if (isProcessingAudio == true) return;
        
        float nowTime = playbackSlider.value;
        
        ////check if need to update slide thumbnails
        //if ((nowTime < slideStartTime) || (nowTime >= slideEndTime)) //need to update waves & pause markers
        //{
        //    //UnityEngine.Debug.Log("need to update wave and pause");
        //    chartStartTime = (int)(startTime / CHART_INTERVAL) * CHART_INTERVAL;

        //    //update pauses
        //    var currentMarkers = new List<GameObject>();
        //    foreach (Transform marker in groupOfPauseMarkers.transform) currentMarkers.Add(marker.gameObject);
        //    currentMarkers.ForEach(marker => Destroy(marker));

        //    //update wave shapes
        //    var currentWaves = new List<GameObject>();
        //    foreach (Transform marker in groupOfWaves.transform) currentWaves.Add(marker.gameObject);
        //    foreach (Transform marker in groupOfPauseMarkers.transform) currentWaves.Add(marker.gameObject);
        //    currentWaves.ForEach(marker => Destroy(marker));

        //    int startFrame = (int)(startTime * FREQUENCY);
        //    int endFrame = startFrame + CHART_INTERVAL * FREQUENCY;
        //    int interval = 8 * CHART_INTERVAL * FREQUENCY / (XBottom - XTop);

        //    int index = 0;
        //    for (int j = startFrame; j < endFrame; j += interval)
        //    {
        //        if (j > floatArray.Length)
        //        {
        //            //draw pause
        //                var go2 = Instantiate(prefabPauseArea) as GameObject;
        //                go2.transform.SetParent(groupOfPauseMarkers.transform);

        //                go2.GetComponent<RectTransform>().localPosition = new Vector3(XTop + 8 * index, YMid, 0f);
        //                go2.GetComponent<RectTransform>().sizeDelta = new Vector2(8, YRange);
        //                go2.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    
        //            break;
        //        }
        //        //get average amplifier
        //        float sum = 0;
        //        for (int k = j; k < j + interval; k++)
        //        {
        //            if (k >= floatArray.Length) break;
        //            sum += Math.Abs(floatArray[k]);
        //        }
        //        float avg = sum / interval;
        //        float max = 0.3f;
        //        float size = avg / max * YRange;
        //        if (size > YRange) size = YRange;

        //        if (avg/floatArrayMaximum < 0.1f)
        //        {
        //            //draw pause
        //            var go2 = Instantiate(prefabPauseArea) as GameObject;
        //            go2.transform.SetParent(groupOfPauseMarkers.transform);

        //            go2.GetComponent<RectTransform>().localPosition = new Vector3(XTop + 8 * index, YMid, 0f);
        //            go2.GetComponent<RectTransform>().sizeDelta = new Vector2(10, YRange);
        //            go2.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        //        }

        //        //instantiate a wave
        //        var go = Instantiate(prefabWave) as GameObject;
        //        go.transform.SetParent(groupOfWaves.transform);

        //        go.GetComponent<RectTransform>().localPosition = new Vector3(XTop + 8 * index, YMid, 0f);
        //        go.GetComponent<RectTransform>().sizeDelta = new Vector2(size, 10);
        //        go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

        //        index++;

        //    }
        //}

        //check if need to update top slide thumbnails
        if ((nowTime<slideStartTime) || (nowTime>slideEndTime)) //now playingtime is out of the slide thumbnail presented time interval
        {
            //find the right group
            int groupNo = 0;
            for (groupNo=0; groupNo<startTimeForThumbnailGroup.Length; groupNo++)
            {
                if (groupNo < startTimeForThumbnailGroup.Length - 1)
                {
                    //Debug.Log("Find right interval-" + groupNo + " comparing " + startTimeForThumbnailGroup[groupNo] + " -- " + nowTime + " -- " + startTimeForThumbnailGroup[groupNo + 1]);
                    if ((nowTime >= startTimeForThumbnailGroup[groupNo]) && (nowTime < startTimeForThumbnailGroup[groupNo + 1]))
                        break;
                }
                else if (nowTime >= startTimeForThumbnailGroup[groupNo])
                {
                    break;
                }
            }

            //page dots
            var oldDots = new List<GameObject>();
            foreach (Transform marker in PageDotGroup.transform) oldDots.Add(marker.gameObject);
            oldDots.ForEach(marker => Destroy(marker));

            if (startTimeForThumbnailGroup.Length == 1) //there's only one page
            {
                var go = Instantiate(pfbPageDot) as GameObject;
                go.transform.SetParent(PageDotGroup.transform);

                go.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
            else
            {
                float space = 20.0f;
                float startPos = (startTimeForThumbnailGroup.Length - 1) * space * 0.5f;

                for (int i=0; i<startTimeForThumbnailGroup.Length; i++)
                {
                    var go = Instantiate(pfbPageDot) as GameObject;
                    go.transform.SetParent(PageDotGroup.transform);

                    go.GetComponent<RectTransform>().localPosition = new Vector3(0, startPos - i * space, 0);
                    go.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    if (i == groupNo)
                        go.GetComponent<ChangeState>().SwitchStateTo(1);
                    else
                        go.GetComponent<ChangeState>().SwitchStateTo(0);
                }
            }

            //update slides
            if (groupNo<startTimeForThumbnailGroup.Length)
            {
                nowGroupNo = groupNo;
                //UnityEngine.Debug.Log("now " + nowGroupNo);
                //found the right interval
                //UnityEngine.Debug.Log("groupNo = " + groupNo);
                slideStartTime = startTimeForThumbnailGroup[groupNo];
                if (groupNo < startTimeForThumbnailGroup.Length - 1)
                    slideEndTime = startTimeForThumbnailGroup[groupNo + 1];
                else
                    if (PresentationData.out_ExitTime > 0)
                        slideEndTime = PresentationData.out_ExitTime;
                    else
                        slideEndTime = 9999999999999.0f;

                startSlideIndex = groupNo * 6;

                //updating slide thumbnail 1~6
                for (int i=0; i<6; i++)
                {
                    int indexOfRecord = groupNo * 6 + i; //updating slide transition record #index
                    if (indexOfRecord >= out_SlidesTransitionRecord.Count-1)
                    {
                        //set this thumbnail to blank
                        slideThumbnails[i].SetActive(false);
                    }
                    else
                    {
                        Texture2D slideTexture = null;
                        if ((PresentationData.out_Slides != null) && (PresentationData.out_Slides.Count > 0))
                            slideTexture = PresentationData.out_Slides[slideTimingRecord[indexOfRecord].Key];
                        else
                            slideTexture = new Texture2D(160, 30);
                        slideThumbnails[i].GetComponentInChildren<ChangeSlideImage>().UpdateImage(slideTexture);
                            
                        //slideThumbnails[i].GetComponentInChildren<ChangeSlideText>().UpdateText("#" + (slideTimingRecord[indexOfRecord].Key+1) + " (" + getTimeString(slideTimingRecord[indexOfRecord].Value) + ")");
                        slideThumbnails[i].GetComponentInChildren<ChangeSlideText>().UpdateText(getTimeString(slideTimingRecord[indexOfRecord].Value));

                        if (slideTimingRecord[indexOfRecord].Key + 1<10)
                            slideThumbnails[i].GetComponentInChildren<ChangeSlideNo>().UpdateText("0"+(slideTimingRecord[indexOfRecord].Key + 1).ToString());
                        else
                            slideThumbnails[i].GetComponentInChildren<ChangeSlideNo>().UpdateText((slideTimingRecord[indexOfRecord].Key + 1).ToString());

                        //if (indexOfRecord <= out_SlidesTransitionRecord.Count - 1)
                        //{
                        //    if ((out_SlidesTransitionRecord[indexOfRecord].Key <= nowTime) && (out_SlidesTransitionRecord[indexOfRecord + 1].Key > nowTime))
                        //    {
                        //        slideThumbnails[i].GetComponentInChildren<ThumbnailFrameController>().SetFrameVisible(true);
                        //        frameStartTime = out_SlidesTransitionRecord[indexOfRecord].Key;
                        //        frameEndTime = frameStartTime + slideTimingRecord[indexOfRecord].Value;
                        //        currentSlide.sprite = Sprite.Create(slideTexture, new Rect(0, 0, slideTexture.width, slideTexture.height), new Vector2(0.5f, 0.5f)); 
                        //        //Debug.Log("updated to slide #" + indexOfRecord+": "+ frameStartTime + "-" + frameEndTime);
                        //    }
                        //    else
                        //        slideThumbnails[i].GetComponentInChildren<ThumbnailFrameController>().SetFrameVisible(false);
                        //}
                        //else
                        //{
                        //    if (out_SlidesTransitionRecord[indexOfRecord].Key < nowTime)
                        //    {
                        //        slideThumbnails[i].GetComponentInChildren<ThumbnailFrameController>().SetFrameVisible(true);
                        //        frameStartTime = out_SlidesTransitionRecord[indexOfRecord].Key;
                        //        frameEndTime = frameStartTime + slideTimingRecord[indexOfRecord].Value;
                        //        currentSlide.sprite = Sprite.Create(slideTexture, new Rect(0, 0, slideTexture.width, slideTexture.height), new Vector2(0.5f, 0.5f)); 
                        //    }
                        //    else
                        //        slideThumbnails[i].GetComponentInChildren<ThumbnailFrameController>().SetFrameVisible(false);
                        //}
                        slideThumbnails[i].SetActive(true);
                    }
                }
            }
            else
                UnityEngine.Debug.Log("Something went wrong");
        }
        
        if ((nowTime<frameStartTime) || (nowTime>=frameEndTime))//check if need to update current slide pointer
        {
            //UnityEngine.Debug.Log("need to update slide frame");
            //UnityEngine.Debug.Log("Updating slide frames --- "+nowTime+" is out of "+frameStartTime+"-"+frameEndTime);
            //Debug.Log("StartSlideIndex=" + startSlideIndex + ", slideTimingRecord.Count=" + slideTimingRecord.Count);
            int nowPageNo = 0;
            
            for (int i=0; i<6; i++)
            {
                if (startSlideIndex + i >= slideTimingRecord.Count)
                {
                    break;
                }
                else
                {
                    //UnityEngine.Debug.Log("debug - record #" + (startSlideIndex + i) + " under processing (out of " + out_SlidesTransitionRecord.Count + " | " + slideTimingRecord.Count + ")");
                    //UnityEngine.Debug.Log("comparing "+ out_SlidesTransitionRecord[startSlideIndex + i].Key + " ?<= "+nowTime+" ?< "+(out_SlidesTransitionRecord[startSlideIndex + i].Key + slideTimingRecord[startSlideIndex + i].Value));
                    if ((nowTime >= out_SlidesTransitionRecord[startSlideIndex + i ].Key) && (nowTime < out_SlidesTransitionRecord[startSlideIndex + i].Key + slideTimingRecord[startSlideIndex + i ].Value))
                    {
                        slideThumbnails[i].GetComponentInChildren<ThumbnailFrameController>().SetFrameVisible(true);
                        frameStartTime = out_SlidesTransitionRecord[startSlideIndex + i].Key;
                        frameEndTime = frameStartTime + slideTimingRecord[startSlideIndex + i].Value;
                        Texture2D slideTexture = null;
                        if ((PresentationData.out_Slides != null) && (PresentationData.out_Slides.Count > 0))
                            slideTexture = PresentationData.out_Slides[slideTimingRecord[startSlideIndex + i].Key];
                        else
                            slideTexture = new Texture2D(160, 30);

                        currentSlide.sprite = Sprite.Create(slideTexture, new Rect(0, 0, slideTexture.width, slideTexture.height), new Vector2(0.5f, 0.5f));
                        if (!currentSlide.gameObject.activeSelf) 
                             currentSlide.gameObject.SetActive(true);
                        nowPageNo = i;
                        currentSlideStartTime.text = getTimeString2(frameStartTime);
                        currentSlideEndTime.text = getTimeString2(frameEndTime);
                        //UnityEngine.Debug.Log("now " + nowGroupNo + "-" + nowPageNo);
                        //Debug.Log("(A)updated to slide #" + (startSlideIndex + i) + ": " + frameStartTime + "-" + frameEndTime);
                    }
                    else
                        slideThumbnails[i].GetComponentInChildren<ThumbnailFrameController>().SetFrameVisible(false);
                }
            }

            //update the wave curve
            groupOfWaves.SetActive(false);
            groupOfPauseMarkers.SetActive(false);

            int noOfWaves = 82;
            float widthOfWaves = 4.01f;
            float leftMost = -93.3f;
            if (waves == null) //first time
            {
                waves = new GameObject[noOfWaves+1];
                
                for (int i=0; i<=noOfWaves; i++)
                {
                    var go = Instantiate(prefabWave) as GameObject;
                    go.transform.SetParent(groupOfWaves.transform);
                    go.GetComponent<RectTransform>().localPosition = new Vector3(leftMost + (widthOfWaves) * i, 58.9f, 0f);
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(widthOfWaves, 47.5f);
                    go.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
                    go.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    waves[i] = go;
                }
            }

            if (pauses == null) //first time
            {
                pauses = new GameObject[noOfWaves + 1];
                for (int i = 0; i <= noOfWaves; i++)
                {
                    var go = Instantiate(prefabPauseArea) as GameObject;  
                    go.transform.SetParent(groupOfPauseMarkers.transform);
                    go.GetComponent<RectTransform>().localPosition = new Vector3(leftMost + (widthOfWaves) * i, 62.1f, 0f);
                    go.GetComponent<RectTransform>().sizeDelta = new Vector2(widthOfWaves, 47.5f);
                    go.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
                    go.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                    pauses[i] = go;
                }
            }

            //update size
            for (int i = 0; i <= noOfWaves; i++)
            {
                //UnityEngine.Debug.Log("Draw wave #" + (6 * nowGroupNo + nowPageNo+i) + " (=" + floatWaveData[6 * nowGroupNo + nowPageNo] + ")");
                float value = (float)floatWaveData[(6*nowGroupNo+nowPageNo)*noOfWaves+i];
                waves[i].GetComponent<RectTransform>().localScale = new Vector3(1f, (value/ floatArrayMaximum) , 1f);
                if (value / floatArrayMaximum < 0.15f)
                    pauses[i].SetActive(true);
                else
                    pauses[i].SetActive(false);
            }

            int countPause = 0;
            //recheck to remove short pauses
            for (int i = 0; i <= noOfWaves; i++ )
            {
                if (pauses[i].activeSelf == true)
                    countPause++;
                else
                {
                    if (countPause < 4)
                    {
                        for (int j = i - countPause; j <= i; j++)
                            pauses[j].SetActive(false);
                    }

                    countPause = 0;
                }
            }

            //int startFrame = (int)(frameStartTime * FREQUENCY);
            //int endFrame = (int)(frameEndTime * FREQUENCY);
            //int interval = (int)((frameEndTime - frameStartTime) * FREQUENCY / noOfWaves);

            //for (int i = 0; i <= noOfWaves; i++ )
            //{
            //    int count=0;
            //    float sum = 0;
            //    for (int j = startFrame+i*interval; j<endFrame; j++)
            //    {
            //        if (j > floatArray.Length - 1)
            //            break;
            //        count++;
            //        sum += Math.Abs(floatArray[j]);
            //    }

            //    waves[i].GetComponent<RectTransform>().localScale = new Vector3(1f, (((float)sum)/(count*floatArrayMaximum))*1.5f, 1f);
            //}

            groupOfWaves.SetActive(true);
            groupOfPauseMarkers.SetActive(true);
        }
        //UnityEngine.Debug.Log(slideStartTime + " <- " + nowTime + " belongs to " + frameStartTime + "-" + frameEndTime + "? --> " + slideEndTime);

        //now update current played position marker
        float positionMarkerLeftMost = -378.9f, positionMarkerRightMost = 379.2f;
        float newPosY = positionMarkerLeftMost - (nowTime - frameStartTime) * (positionMarkerLeftMost - positionMarkerRightMost) / (frameEndTime - frameStartTime);
        if (newPosY <= positionMarkerRightMost)
            currentPositionMarker.GetComponent<RectTransform>().localPosition = new Vector3(0, newPosY, 0);
        else
            currentPositionMarker.GetComponent<RectTransform>().localPosition = new Vector3(0, positionMarkerRightMost, 0);
    }

	// Use this for initialization
	void Start () {
        Debug.Log("Enter Eval Scene");
        //set volume control
        //#if UNITY_ANDROID 		
        //        unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //        currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
        //        currentActivity.Call("ChangeVolumeControl");
        //        Debug.Log("Tried to active volume button control");
        //#endif
        //setup heatmap
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        heatMapGen = this.GetComponent<HeatmapGenerator>();
        Texture2D tempTex;
        float maxTime;
        float outOfBoundRatio;
        if (PresentationData.out_HGGazeData != null)
        {
            heatMapGen.GenerateMap(PresentationData.out_HGGazeData, 0, PresentationData.out_ExitTime, out tempTex, out maxTime, out outOfBoundRatio);
            heatmapHolder.GetComponent<Image>().sprite = Sprite.Create(tempTex, new Rect(0, 0, tempTex.width, tempTex.height), new Vector2(0.5f, 0.5f));
            screenshotHolder.GetComponent<Image>().sprite = Sprite.Create(PresentationData.out_Screenshot, new Rect(0, 0, PresentationData.out_Screenshot.width, PresentationData.out_Screenshot.height), new Vector2(0.5f, 0.5f));
        }

        //set up audio source
        if (audioSource == null)
        {
            //UnityEngine.Debug.Log("get audio source"); 
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        if (audioSource.clip != null)
            audioSource.clip = null;
        
        pcmToUnityClip = new AudioProcessingJob();
        if ((PresentationData.out_RecordingFilePath != null) && (PresentationData.out_RecordingFilePath != ""))
            pcmToUnityClip.setUpFile(PresentationData.out_RecordingFilePath);
        else
            pcmToUnityClip.setUpFile(@"C:\Users\xunchis\record.pcm");
            //pcmToUnityClip.setUpFile(@"C:\Users\jaekyunk\Desktop\VR_Rehearsal\VR_Rehearsal_app\record.pcm");

        //set up slide transition data
        out_SlidesTransitionRecord = new List<KeyValuePair<float, int>>();

        if ((PresentationData.out_SlidesTransitionRecord != null) && (PresentationData.out_SlidesTransitionRecord.Count >= 0))
            out_SlidesTransitionRecord = PresentationData.out_SlidesTransitionRecord;
        else
        {
            //give it some test data
            out_SlidesTransitionRecord = new List<KeyValuePair<float, int>>();
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(0.0f, 0));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(1.0f, 1));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(5.0f, 2));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(10.0f, 3));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(15.0f, 4));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(20.0f, 5));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(22.0f, 6)); //the last shoot
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(26.0f, 7));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(35.0f, 8));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(55.0f, 9));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(70.0f, 10));
            out_SlidesTransitionRecord.Add(new KeyValuePair<float, int>(82.0f, 11));
        }

        //prepare duration data
        slideTimingRecord = new List<KeyValuePair<int, float>>();

        //for (int j = 0; j < out_SlidesTransitionRecord.Count; j++ )
        //{
        //    UnityEngine.Debug.Log(out_SlidesTransitionRecord[j].Key + " + " + out_SlidesTransitionRecord[j].Value);
        //}

        //UnityEngine.Debug.Log("brain power!! " + out_SlidesTransitionRecord.Count + " = " + (out_SlidesTransitionRecord.Count-1/6) + ")");
        startTimeForThumbnailGroup = new float[(out_SlidesTransitionRecord.Count-2)/ 6+1];
        //UnityEngine.Debug.Log("startTimeForThumbnailGroup has " + (out_SlidesTransitionRecord.Count - 2) / 6 + "+1 records");
        for (int i = 0; i < out_SlidesTransitionRecord.Count-1; i++)
        {
            float time = out_SlidesTransitionRecord[i].Key;
            int slideNo = out_SlidesTransitionRecord[i].Value;

           //get the duration
            float dur = out_SlidesTransitionRecord[i + 1].Key - time;
            slideTimingRecord.Add(new KeyValuePair<int, float>(slideNo, dur));
            if (i % 6 == 0)
            {
                //Debug.Log("*"+i+"* Group #" + (i / 6) + " starts on " + out_SlidesTransitionRecord[i].Key);
                startTimeForThumbnailGroup[i / 6] = out_SlidesTransitionRecord[i].Key;
            }

            //UnityEngine.Debug.Log("transition is picked: " + dur + " (#" + slideNo + ")");
        }
        pcmToUnityClip.setUpSlideTransitionData(slideTimingRecord);
        
        pcmToUnityClip.Start();
        isProcessingAudio = true;
	}
    private void CustomApplicationQuit()
    {
#if UNITY_EDITOR
        Application.Quit();
#elif UNITY_ANDROID
         using (AndroidJavaClass javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject unityActivity = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
            unityActivity.Call<bool>("moveTaskToBack", true);
        }
#endif
    }
    // Update is called once per frame
    void Update () {
        preventTrigger = true;
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && this.gameObject.activeSelf)
            {
                Debug.Log("Done!!");

                CustomApplicationQuit();
            }

        }
        if (isProcessingAudio == true)
        {
            if (pcmToUnityClip.IsDone == false) //show progress
            {
                if (arrayLength == 0)
                    arrayLength = pcmToUnityClip.endvalue;
                else
                {
                    float widthRate = 1-(((float)pcmToUnityClip.progress) / ((float)arrayLength));
                    loadingBar.GetComponent<RectTransform>().sizeDelta = new Vector2((int)(widthRate * 800.0f), 10);
                    //UnityEngine.Debug.Log(pcmToUnityClip.progress + "/" + arrayLength);
                    //UnityEngine.Debug.Log(pcmToUnityClip.progressWave);
                }
            }
            else //prepare data, show slider
            {
                loadingText.SetActive(false);
                loadingGroup.SetActive(false);

                isProcessingAudio = false;
                floatArray = pcmToUnityClip.getArray();
                floatWaveData = pcmToUnityClip.getWaveData();
                floatArrayMaximum = pcmToUnityClip.getMaximum();
                //UnityEngine.Debug.Log("maximum is " + floatArrayMaximum);
                //UnityEngine.Debug.Log(floatArray.Length);
                UnityEngine.Debug.Log(floatWaveData.Length);

                //time stamp update
                float totaltime = (floatArray.Length) / (float)FREQUENCY;
                quarterTime.text = getTimeString2(totaltime / 4.0f);
                halfTime.text = getTimeString2(totaltime / 2.0f);
                softTime.text = getTimeString2(totaltime * 0.75f);
                endTime.text = getTimeString2(totaltime);

                AudioClip myClip = AudioClip.Create("record", floatArray.Length, 1, FREQUENCY, false, false);
                myClip.SetData(floatArray, 0);
                audioSource.clip = myClip;
            
                preventTrigger = true;
                playbackSlider.direction = Slider.Direction.LeftToRight;
                playbackSlider.minValue = 0;
                playbackSlider.maxValue = audioSource.clip.length;
                playbackSlider.value = 0;
                preventTrigger = false;

                groupReplayObjects.SetActive(true);
            }
            
        }
        else
            RefreshTopChart();

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

public class AudioProcessingJob : ThreadedAudioJob
{
    private string filePath;
    private const int FREQUENCY = 44100;
    private float[] floatArray;
    private float[] floatForWaves;
    private float floatArrayMaximum = 0.0f;
    public int progress = 0;
    public int progressWave = 0;
    public int endvalue = 0;
    private List<KeyValuePair<int, float>> slideTiming;

    public void setUpFile(string filename)
    {
        filePath = filename;
    }

    public void setUpSlideTransitionData(List<KeyValuePair<int, float>> data)
    {
        slideTiming = data;
    }

    public float[] getArray()
    {
        return floatArray;
    }

    public float[] getWaveData()
    {
        return floatForWaves;
    }

    public float getMaximum()
    {
        return floatArrayMaximum;
    }

    protected override void ThreadFunction()
    {
        byte[] byteArray = null;
#if !UNITY_EDITOR && UNITY_ANDROID
        if ((PresentationData.out_RecordingFilePath != null) && (PresentationData.out_RecordingFilePath != ""))
            try { 
                byteArray = File.ReadAllBytes(PresentationData.out_RecordingFilePath); 
            }
            catch (FileNotFoundException e) { }
        else
        {
#endif
        //byteArray = File.ReadAllBytes(@"C:\Users\jaekyunk\Desktop\VR_Rehearsal\VR_Rehearsal_app\record.pcm"); //for testing
        byteArray = File.ReadAllBytes(@"C:\Users\xunchis\record.pcm"); //for testing
#if !UNITY_EDITOR && UNITY_ANDROID
        }
#endif
        //byte > unity float
        if (byteArray == null)
        {
            //UnityEngine.Debug.Log("File not found");
            return;
        }
        endvalue = byteArray.Length / 2;
        floatArray = new float[byteArray.Length / 2 + 1];
        floatForWaves = new float[floatArray.Length];

        int currentSlideIndex = 0;
        int currentSlideStartFrame = 0;
        int currentSlideEndFrame = currentSlideStartFrame+(int)(slideTiming[currentSlideIndex].Value*FREQUENCY);
        //int interval = 0;
        int noOfWaves = 82;

        int currentWaveStartFrame = 0;
        int currentWaveEndFrame = currentWaveStartFrame+(currentSlideEndFrame - currentSlideStartFrame) / noOfWaves;
        int currentWaveFrameIndex = 0;

        float currentSum = 0;
        int currentCount = 0;
        int i;
        floatArrayMaximum = 0.0f;
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
            
            //translate to -1.0~1.0f
            float valueF = ((float)valueS) / 32768.0f;
            //if (valueF > floatArrayMaximum)
            //    floatArrayMaximum = valueF;
            floatArray[i / 2] = valueF;
            if (i % 100000 == 0) progress = i/2;

            if (currentSlideIndex < slideTiming.Count)
            {//prepare wave 
                currentSum += Math.Abs(valueF);
                currentCount++;

                if ((i / 2 == currentWaveEndFrame) || (i == byteArray.Length - 1) || (i == byteArray.Length - 2)) //end of current wave object
                {
                    //get avg
                    float newValue = currentSum / (float)currentCount;
                    floatForWaves[currentWaveFrameIndex] = newValue;
                    if (newValue > floatArrayMaximum)
                        floatArrayMaximum = newValue;
                    //reset
                    currentSum = 0f;
                    currentCount = 0;
                    //get next wave start/end
                    currentWaveStartFrame = currentWaveEndFrame + 1;
                    currentWaveEndFrame = currentWaveStartFrame + (currentSlideEndFrame - currentSlideStartFrame) / noOfWaves; //start+interval
                    currentWaveFrameIndex++;
                    progressWave = currentWaveFrameIndex;
                }

                if (currentWaveStartFrame > currentSlideEndFrame)
                {
                    currentSlideIndex++;
                    if (currentSlideIndex < slideTiming.Count)
                    {
                        currentSlideStartFrame = currentSlideEndFrame + 1;
                        currentSlideEndFrame = currentSlideStartFrame + (int)(slideTiming[currentSlideIndex].Value * FREQUENCY);
                        currentWaveStartFrame = currentSlideStartFrame;
                        currentWaveEndFrame = currentWaveStartFrame + (currentSlideEndFrame - currentSlideStartFrame) / noOfWaves;
                    }
                }
            }
        }
        floatArray[i / 2] = '\0';
    }

    protected override void OnFinished()
    {
    }
}