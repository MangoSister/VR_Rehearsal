using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class EvaluationUI_Manager : MonoBehaviour {

	//Record and play
	public AudioSource audioSource;
	private float tElapsed = 0f;

	//Heatmap
	private HeatmapGenerator heatMapGen;
	public GameObject heatmapHolder;
	public GameObject screenshotHolder;
	// Use this for initialization
	void Start () {
		heatMapGen = this.GetComponent<HeatmapGenerator> ();
		Texture2D tempTex;
		float maxTime;
		heatMapGen.GenerateMap (PresentationData.out_HGGazeData, 0, PresentationData.out_ExitTime, out tempTex, out maxTime);

		heatmapHolder.GetComponent<Image> ().sprite = Sprite.Create (tempTex, new Rect(0,0,tempTex.width, tempTex.height), new Vector2(0.5f, 0.5f));
		screenshotHolder.GetComponent<Image> ().sprite = Sprite.Create (PresentationData.out_Screenshot, new Rect(0,0,PresentationData.out_Screenshot.width, PresentationData.out_Screenshot.height), new Vector2(0.5f, 0.5f));

	}

	public void HearReplay()
	{
		

		if (audioSource == null)
		{
			UnityEngine.Debug.Log("get audio source"); 
			audioSource = gameObject.GetComponent<AudioSource>();
		}

		if (audioSource.clip != null)
			audioSource.clip = null;

		{
			UnityEngine.Debug.Log("read from file"); 

			byte[] byteArray = File.ReadAllBytes( Application.persistentDataPath + "/record.pcm"); //change this

			//PCM file is 16bit PCM, need byte > unity float transform
			// UnityEngine.Debug.Log("size = " + byteArray.Length);

			float[] floatArr = new float[byteArray.Length / 2+1];
			int i;
			short max = 0;
			for (i = 0; i < byteArray.Length; i=i+2) 
			{
				//1 short = 2 bytes = 16 bit & in little endian
				byte[] bytebuffer = new byte[2];
				if (BitConverter.IsLittleEndian == true)
				{     
					bytebuffer[0] = byteArray[i];
					bytebuffer[1] = byteArray[i+1];
				}
				else
				{
					bytebuffer[0] = byteArray[i+1];
					bytebuffer[1] = byteArray[i];
				}

				short valueS = BitConverter.ToInt16(bytebuffer, 0);
				if (valueS > max) max = valueS;
				//translate to -1.0~1.0f

				float valueF = ((float)valueS * 20f) / 32768.0f; //20 is a multipler for amplify the volume
				floatArr[i / 2] = valueF;
			}
			floatArr[i / 2] = '\0';
			//UnityEngine.Debug.Log("max "+max);

			AudioClip myClip = AudioClip.Create("record", floatArr.Length, 1, 8000, false, false); //8000 and 1 coming from Byunghwan's setting in native Android side
			myClip.SetData(floatArr, 0);
			audioSource.clip = myClip;
			// UnityEngine.Debug.Log("reset slider"); 
			//playbackSlider.direction = Slider.Direction.LeftToRight;
			//playbackSlider.minValue = 1;
			//playbackSlider.maxValue = audioSource.clip.length;
			//playbackSlider.value = 0;
		}

		//UnityEngine.Debug.Log("change button text and start playing"); 
		//playbackButtonText.text = "Stop";
		audioSource.Play();
		//updateFlag = 0;

		//for (int numOfMarker = 0; numOfMarker < 2; numOfMarker++)
		//    Instantiate(ButtonJumpTo, new Vector3(0, 0, 0), Quaternion.identity);

		//JumpPlaybackButtonModule[] jmpList = UnityEngine.Object.FindObjectsOfType<JumpPlaybackButtonModule>();

		//for (int index = 0; index < jmpList.Length; index++)
		//{
		//    jmpList[index].transform.parent = UnityEngine.Object.FindObjectOfType<Canvas>().transform;
		//    jmpList[index].SetUp(2.0f + index * 5.0f, playbackSlider);
		//}
	}


	// Update is called once per frame
	void Update () {
	
	}
}
