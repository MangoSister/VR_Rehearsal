/*
 * Phan Lee 
 * modified by Yang
 * Updated:Feb 07, 2016
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
	private Coroutine _constraintCR = null;

    void Start()
    {
        //_prevRotQuat Get original rotation 
        _prevRotQuat = boneTransform.rotation;
    }
	
	//Start Constarint mode
	public void StartToFollow()
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
	public void StopToFollow()
	{
        if (_constraintCR != null)
            StopCoroutine(_constraintCR);
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