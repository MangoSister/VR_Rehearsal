using UnityEngine;
using System.Collections;

public class Swipe_Input : MonoBehaviour {

	/*Swipe Detection */
	public float minSwipeDistY;
	public float minSwipeDistX;
	private Vector2 _startPos;
	/*----------------*/


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		DetectSwipeDirection ();
	}

	void DetectSwipeDirection(){
		if (Input.touchCount > 0) {
			Touch touch = Input.touches [0];

			switch (touch.phase) {
			case TouchPhase.Began:
				{
					_startPos = touch.position;	
				}
				break;

			case TouchPhase.Ended:
				{	
					//1. Vertical swipe Checking
					float swipeDistVertical = (new Vector3 (0, touch.position.y, 0) - new Vector3 (0, _startPos.y, 0)).magnitude;

					if (swipeDistVertical > minSwipeDistY) {
						float swipeValue = Mathf.Sign (touch.position.y - _startPos.y);
						if (swipeValue > 0) {
							/*Up*/
						} else if (swipeValue < 0) {
							/*Down*/
						}
					}

					//2. Horizontal swipe Checking
					float swipeDistHorizontal = (new Vector3 (touch.position.x, 0, 0) - new Vector3 (_startPos.x, 0, 0)).magnitude;

					if (swipeDistHorizontal > minSwipeDistX) {
						float swipeValue = Mathf.Sign (touch.position.x - _startPos.x);
						if (swipeValue > 0) {
							/*Right*/
						} else if (swipeValue < 0) {
							/*Left*/
						}
					}
				}
				break;

			}

		}
	}
}
