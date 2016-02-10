/*
 * Byunghwan Lee Feb.9 2016
 * Turoial Manager. 
 * It will be fixed later.
 * 
 */ 
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class TutorialManager : MonoBehaviour {

	//Structure for saving each slide session info
	//------------------------------------------------------------------------------------------------
	public class TutSlide{
		//Slide Textures
		public Sprite[] sprites;
		//when we set a playtime without event, it means entirle play time.
		//but if we set this with event, it means animation interval time.
		public float playTime;
		//Flag for having animation or not
		public bool isAnimation;
		//Flag for checking Event signal
		public bool eventFlag;
		//nullable type for enumurator 
		public VRInputManager.TutorialInputType? _inputType = null;
		
		//Initialize function, if want to make non event type, just set _type as null
		public void Initialize(string[] slideName, VRInputManager.TutorialInputType? _type, bool isAnimation, float playTime ){
			LoadSlides (slideName);
			if (_type != null) {
				_inputType = _type;
			} 
			isAnimation = isAnimation;   playTime = playTime;  eventFlag = false;
		}
		
		//Load slides from Asset/Resources folder.
		void LoadSlides(string[] slideName){
			sprites = new Sprite[slideName.Length];
			for (int i = 0; i < slideName.Length; ++i) {
				Sprite tempSprite = Resources.Load<Sprite>( slideName[i]);
				
				if(tempSprite != null){
					sprites[i] = tempSprite;
				}else{
					Debug.LogError("[Error] There is no such a texture file");
				}
			}
		}
		
		public void SetOnEventFlag(){
			eventFlag = true;
		}
	}
	//---------------------------------------------------------------------------------------------------
	private SpriteRenderer spriteRend;
	public string tutorialName;
	private TutSlide[] _slides;
	private int _currIdx;

	// Use this for initialization
	void Start () {

		// I am gonaa change later this structure ~~~~ QUE~~~~~~~
		// 
		spriteRend = this.gameObject.GetComponent<SpriteRenderer> ();
		tutorialName = "pptKaraoke";

		_slides = new TutSlide[9];

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
		arr1 = new string[] { "pptKaraoke/t4"};
		_slides[3].Initialize( arr1, null, false, 3.0f);

		_slides[4] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t5", "pptKaraoke/t5_1"};
		_slides[4].Initialize( arr1, null, true, 3.0f);

		_slides[5] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t7"};
		_slides[5].Initialize( arr1, null, false, 3.0f);

		_slides[6] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t8"};
		_slides[6].Initialize( arr1, null, false, 3.0f);

		_slides[7] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t9"};
		_slides[7].Initialize( arr1, null, false, 3.0f);

		_slides[8] = new TutSlide();
		arr1 = new string[] { "pptKaraoke/t10"};
		_slides[8].Initialize( arr1, null, false, 3.0f);

		Draw (_slides [_currIdx]);
	}

	public void ChangeSlide(Sprite slide){
		spriteRend.sprite = slide;
	}
	
	public void GoNextSlide(){
		if (++_currIdx < _slides.Length) {
			Draw (_slides [_currIdx]);
		} else {
			ChangeSlide(null);
		}
	}
	
	//Change Slide and Draw 
	public void Draw(TutSlide slide){
		
		if (slide._inputType.HasValue == false) { /* Non Event Type */
			if (!slide.isAnimation) {
				// Default Draw slide
				ChangeSlide (slide.sprites[0]);
				StartCoroutine (DrawDefalut_CR (slide));
				
			} else {
				//	Animation Slides play once
				StartCoroutine (DrawAnimation_CR (slide));
			}
		} else { /* Event Type */
			if (!slide.isAnimation) {
				ChangeSlide (slide.sprites [0]);
				StartCoroutine (DrawDefalutUnitlEvent_CR (slide));
			} else {
				//	Animation Slides play loop
				StartCoroutine (DrawAnimationUnilEvent_CR (slide));
			}
		}
	}
	
	
	// Type of play each slide 
	// 1. default draw -> change slide from sprite and then wait for a second and, go next page
	IEnumerator DrawDefalut_CR(TutSlide slide){
		yield return new WaitForSeconds (slide.playTime);
		GoNextSlide ();
	}
	// 2. animation -> change slide sequentially and then go next page.
	IEnumerator DrawAnimation_CR(TutSlide slide){

		float currTime = 0;
		float totalTime = 0;
		int idx = 0;
		float intervalTime = (slide.playTime / slide.sprites.Length) /2;
		ChangeSlide(slide.sprites[idx]);

		while(totalTime < slide.playTime){
			currTime += Time.deltaTime;
			totalTime +=  Time.deltaTime;
			if(currTime > intervalTime ){
				currTime = 0;
				idx = (idx + 1) % ( slide.sprites.Length );
				ChangeSlide(slide.sprites[idx]);
			}
			yield return null;
		}

		GoNextSlide ();
	}

	// 3. Wait unitl get a Event from VRInputManager. 
	IEnumerator DrawDefalutUnitlEvent_CR(TutSlide slide){
		if (slide._inputType.HasValue == false)
			yield break;
		//Register Event to VRInputManager
		VRInputManager.currTutInputManager.RegisterEvent (slide._inputType.Value, slide.SetOnEventFlag);
		
		while(true){
			yield return null;
			if(slide.eventFlag == true){
				break;
			}
			
		}
		//UnRegister Event to VRInputManager
		VRInputManager.currTutInputManager.UnRegisterEvent (slide._inputType.Value, slide.SetOnEventFlag);
		GoNextSlide ();
	}
	
	// 4. Wait unitl get a Event from VRInputManager while looping animation.
	IEnumerator DrawAnimationUnilEvent_CR(TutSlide slide){
		if (slide._inputType.HasValue == false)
			yield break;
		
		float currTime = 0;
		int idx = 0;
		ChangeSlide(slide.sprites[idx]);
		VRInputManager.currTutInputManager.RegisterEvent (slide._inputType.Value, slide.SetOnEventFlag);
		
		while(true){
			currTime += Time.deltaTime;

			if(currTime > slide.playTime ){
				currTime = 0;
				idx = (idx + 1) % ( slide.sprites.Length  );
				ChangeSlide(slide.sprites[idx]);
			}
			yield return null;
			if(slide.eventFlag == true){
				break;
			}
			
		}
		VRInputManager.currTutInputManager.UnRegisterEvent (slide._inputType.Value, slide.SetOnEventFlag);
		GoNextSlide ();
	}



}
