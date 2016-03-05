using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EvaluationUI_Manager : MonoBehaviour {
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
	
	// Update is called once per frame
	void Update () {
	
	}
}
