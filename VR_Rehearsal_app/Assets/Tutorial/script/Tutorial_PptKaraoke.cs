using UnityEngine;
using System.Collections;

public class Tutorial_PptKaraoke : Tutorial_Base {

	// Use this for initialization
	protected override void Initialize(){
		tutorialName = "pptKaraoke";
		
		//Configuration
		slidePlayer.enabled = false;
		timerPlayer.enabled = false;
		
		_slides = new TutSlide[8];
		
		_slides[0] = new TutSlide();
		string[] arr1 = new string[] { "pptKaraoke/t1" };
		_slides[0].Initialize( arr1, null, false, 3.0f);
		
		_slides[1] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t2"};
		_slides[1].Initialize( arr1, null, false, 3.0f);
		
		_slides[2] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t3"};
		_slides[2].Initialize( arr1, VRInputManager.TutorialInputType.LookAtScreen, false, 0);
		
		_slides[3] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t5", "pptKaraoke/t5_1"};
		_slides[3].Initialize( arr1, null, true, 3.0f);
		
		_slides[4] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t7"};
		_slides[4].Initialize( arr1, null, false, 3.0f);
		
		_slides[5] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t8"};
		_slides[5].Initialize( arr1, null, false, 3.0f);
		
		_slides[6] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t9"};
		_slides[6].Initialize( arr1, null, false, 3.0f);
		
		_slides[7] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t10"};
		_slides[7].Initialize( arr1, null, false, 3.0f);
		
	}
}
