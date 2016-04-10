/*
 * Byunghwan Lee Feb.9 2016
 * Turoial Manager. 
 * It will be fixed later.
 * 
 */ 
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class Tutorial_Base : MonoBehaviour {

    public SlidesPlayer slidePlayer;
    public ClockTimer timerPlayer;

	protected SpriteRenderer spriteRend;
	public string tutorialName;
	protected TutSlide[] _slides;
	protected int _currIdx;

	// Use this for initialization
	protected virtual void Start () {
		// I am gonaa change later this structure ~~~~ QUE~~~~~~~
		spriteRend = this.gameObject.GetComponent<SpriteRenderer> ();
		Initialize ();

		if (_slides.Length > 0)
			Draw (_slides [_currIdx]);
		else {
			#if UNITY_EDITOR
			Debug.Log("[Error] Tutorial_ " +tutorialName);
			#endif
		}
	}

	protected virtual void Initialize (){}


	public void ChangeSlide(Sprite slide){
		spriteRend.sprite = slide;
	}
	
	public void GoNextSlide(){
		if (++_currIdx < _slides.Length) {
            Draw(_slides[_currIdx]);
           
		} else {
			ChangeSlide(null);

            //Configuration
            slidePlayer.enabled = true;
            timerPlayer.enabled = true;
        }
	}
	
	//Change Slide and Draw 
	public void Draw(TutSlide slide){
		if (slide._inputType.HasValue == false) { /* Non Event Type */
			if (!slide._isAnimation) {
				// Default Draw slide
				ChangeSlide (slide._sprites[0]);
				StartCoroutine (DrawDefalut_CR (slide));
				
			} else {
				//	Animation Slides play once
				StartCoroutine (DrawAnimation_CR (slide));
			}
		} else { /* Event Type */
			if (!slide._isAnimation) {
				ChangeSlide (slide._sprites[0]);
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
		yield return new WaitForSeconds (slide._playTime);
		GoNextSlide ();
	}
	// 2. animation -> change slide sequentially and then go next page.
	IEnumerator DrawAnimation_CR(TutSlide slide){

		float currTime = 0;
		float totalTime = 0;
		int idx = 0;
		float intervalTime = (slide._playTime / slide._sprites.Length) /2;
		ChangeSlide(slide._sprites[idx]);

		while(totalTime < slide._playTime)
        {
			currTime += Time.deltaTime;
			totalTime +=  Time.deltaTime;
			if(currTime > intervalTime ){
				currTime = 0;
				idx = (idx + 1) % ( slide._sprites.Length );
				ChangeSlide(slide._sprites[idx]);
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
			if(slide._eventFlag == true){
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
		ChangeSlide(slide._sprites[idx]);
		VRInputManager.currTutInputManager.RegisterEvent (slide._inputType.Value, slide.SetOnEventFlag);
		
		while(true){
			currTime += Time.deltaTime;

			if(currTime > slide._playTime)
            {
				currTime = 0;
				idx = (idx + 1) % ( slide._sprites.Length  );
				ChangeSlide(slide._sprites[idx]);
			}
			yield return null;
			if(slide._eventFlag == true){
				break;
			}
			
		}
		VRInputManager.currTutInputManager.UnRegisterEvent (slide._inputType.Value, slide.SetOnEventFlag);
		GoNextSlide ();
	}


	//Structure for saving each slide session info
	//------------------------------------------------------------------------------------------------
	public class TutSlide{
		//Slide Textures
		public Sprite[] _sprites;
		//when we set a playtime without event, it means entirle play time.
		//but if we set this with event, it means animation interval time.
		public float _playTime;
		//Flag for having animation or not
		public bool _isAnimation;
		//Flag for checking Event signal
		public bool _eventFlag;
		//nullable type for enumurator 
		public VRInputManager.TutorialInputType? _inputType = null;
		
		//Initialize function, if want to make non event type, just set _type as null
		public void Initialize(string[] slideName, VRInputManager.TutorialInputType? _type, bool isAnimation, float playTime ){
			LoadSlides (slideName);
			if (_type != null) {
				_inputType = _type;
			}
			_isAnimation = isAnimation; _playTime = playTime; _eventFlag = false;
		}
		
		//Load slides from Asset/Resources folder.
		void LoadSlides(string[] slideName){
			_sprites = new Sprite[slideName.Length];
			for (int i = 0; i < slideName.Length; ++i) {
				Sprite tempSprite = Resources.Load<Sprite>( slideName[i]);
				
				if(tempSprite != null){
					_sprites[i] = tempSprite;
				}else{
					#if UNITY_EDITOR
					Debug.LogError("[Error] There is no such a texture file");
					#endif
				}
			}
		}
		
		public void SetOnEventFlag(){
			_eventFlag = true;
		}
	}
	//---------------------------------------------------------------------------------------------------
}
