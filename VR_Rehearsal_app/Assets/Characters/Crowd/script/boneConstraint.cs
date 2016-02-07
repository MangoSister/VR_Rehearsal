/*
 * Phan Lee 
 * Updated:Jan 24, 2016
 * 
 * Bone Constraint Function
 * later, i will add constraint angle limitation function
*/
using UnityEngine;
using System.Collections;

public class boneConstraint : MonoBehaviour 
{
	public Transform boneTransform;
	public Transform targetTransform;
	//save previous rotation for moving back to non-constraint mode
	private Quaternion _prevRotQuat;
	//After attention, 
	private Coroutine _constraintCR;

    void Start()
    {
        //_prevRotQuat Get original rotation 
    }


    void Update () 
	{
		
		//boneTransform.LookAt(targetTransform );
		if (Input.GetKeyDown(KeyCode.A)) {  /* Send Event */
			StartToFollow();
		}
		if (Input.GetKeyDown(KeyCode.B)) {  /* Send Event */
			StopToFollow();
		}
	}
	
	//Start Constarint mode
	void StartToFollow()
	{
		// Get vector between Target and bone  
		Vector3 relativePos = targetTransform.position - boneTransform.position;
		// Get LookAt dir vector
		Quaternion rotation = Quaternion.LookRotation (relativePos, Vector3.up);
		// Save currVector in order to go back release status;
		_prevRotQuat = boneTransform.rotation;
		// Start Quaternion Lerp Coroutine
		StartCoroutine ( LerpRotBetweenAnB_CR (boneTransform.rotation, rotation, true));
	}
	
	//Stop Constarint mode
	void StopToFollow()
	{
		StopCoroutine (_constraintCR);
		StartCoroutine (LerpRotBetweenAnB_CR (boneTransform.rotation, _prevRotQuat, false));
	}
	
	// Lerp for moving smoothly
	IEnumerator LerpRotBetweenAnB_CR(Quaternion quatA, Quaternion quatB, bool isConstraintMode)
	{
		float time = 0.0f;
		while (time < 1.5f) {
			time += Time.deltaTime;
			boneTransform.rotation = Quaternion.Slerp (quatA, quatB, time);
			yield return new WaitForEndOfFrame();
		}
		
		if (isConstraintMode) {
			_constraintCR = StartCoroutine(KeepConstraint_CR());
		}
	}
	
	//Make bone keep following to target
	IEnumerator KeepConstraint_CR()
	{
		while (true)
		{
			boneTransform.LookAt(targetTransform);
			yield return new WaitForEndOfFrame();
		}
		
	}
	
	
}