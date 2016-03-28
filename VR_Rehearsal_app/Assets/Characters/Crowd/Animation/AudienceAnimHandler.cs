/* AudienceAnimHandler.cs
 * Yang Zhou, last modified on Feb 09, 2016
 * AudienceAnimHandler updates character animation as well as targeting behavior
 * Dependencies: character animator, and Audience.cs
 */

using UnityEngine;
using System.Collections;

public class AudienceAnimHandler : MonoBehaviour
{
    private static int _paramIdState;
    private static int _paramIdFaceDirX;
    private static int _paramIdFaceDirZ;
    private static int _paramIdSwitchPose;

    AudienceAnimHandler()
    {
        _paramIdState = Animator.StringToHash("state");
        _paramIdFaceDirX = Animator.StringToHash("faceDirX");
        _paramIdFaceDirZ = Animator.StringToHash("faceDirZ");
        _paramIdSwitchPose = Animator.StringToHash("switchPose");
    }

    private Audience _audience;
    public Animator anim;

    //Random period to switch animation for variation
    public Vector2 repeatPeriodBound = new Vector2(3f, 8f);

    //Layer lerping speed during following transition
    public float LerpAnimLayerSpeed = 1.0f;
    
    //Neck rotatin angular speed during following transition
    public float SwitchFollowDegSpeed = 60f;
    private Transform _currTarget;
    //private bool _isFollowing = false;
    private Quaternion _defaultHeadLocalRotation;

    private Coroutine _currStartFollowCR = null;
    private Coroutine _currStopFollowCR = null;
    private Coroutine _currLookAtCR = null;

    //Animator layer index, must be consistent with the actual animator
    private const int neckConstraintLayerIdx = 0;
    private const int defaultLayerIdx = 1;
    public int blabla = 0;
    //Use me to start following target!
    //will do nothing if the audience has already been in following state
    //will do nothing during transition
    public void StartToFollow(Transform target)
    {
       
        //if (_isFollowing /*|| _isTransiting*/)
        //    return;
        _defaultHeadLocalRotation = _audience.headTransform.localRotation;
        if (_currStartFollowCR != null)
            StopCoroutine(_currStartFollowCR);
        if (_currStopFollowCR != null)
            StopCoroutine(_currStopFollowCR);
        if (_currLookAtCR != null)
            StopCoroutine(_currLookAtCR);
        blabla = 1;
        _currStartFollowCR = StartCoroutine(StartToFollow_CR(target));
    }

    //Use me to stop following!
    //will do nothing if the audience has already been out of following state
    //will do nothing during transisiton
    public void StopToFollow()
    {
        if (_currStartFollowCR != null)
            StopCoroutine(_currStartFollowCR);
        if (_currStopFollowCR != null)
            StopCoroutine(_currStopFollowCR);
        if (_currLookAtCR != null)
            StopCoroutine(_currLookAtCR);
        blabla = 0;
        _currStopFollowCR = StartCoroutine(StopToFollow_CR());
    }

    private IEnumerator StartToFollow_CR(Transform target)
    {
        //1. gradually switch animator layer
        float initWeight = anim.GetLayerWeight(defaultLayerIdx);
        float timeElapsed = 0f;
        float totalTime = Mathf.Abs(0f - initWeight) / LerpAnimLayerSpeed;
        while (timeElapsed < totalTime)
        {
            anim.SetLayerWeight(defaultLayerIdx,
                Mathf.Lerp(initWeight, 0f, Mathf.Clamp01(timeElapsed / totalTime)));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        anim.SetLayerWeight(defaultLayerIdx, 0f);

        //2. lerp neck rotation to follow target
        Quaternion lookRot;
        do
        {
            lookRot = Quaternion.LookRotation
                ((target.position - _audience.headTransform.position).normalized, Vector3.up);
            _audience.headTransform.rotation = Quaternion.RotateTowards
                (_audience.headTransform.rotation, lookRot, SwitchFollowDegSpeed * Time.deltaTime);
            yield return null;
        }
        while (Quaternion.Angle(_audience.headTransform.rotation, lookRot) > 0.5f);
        _audience.headTransform.rotation = lookRot;

        _currTarget = target;
        if (_currTarget != null)
           _currLookAtCR = StartCoroutine(LookAt_CR());

        _currStartFollowCR = null;
    }

    private IEnumerator StopToFollow_CR()
    {
        if (_currLookAtCR != null)
        {
            StopCoroutine(_currLookAtCR);
            _currLookAtCR = null;
        }

        _currTarget = null;
        //1. lerp neck rotation to default
        while (Quaternion.Angle(_defaultHeadLocalRotation,
            _audience.headTransform.localRotation) > 0.5f)
        {
            _audience.headTransform.localRotation = Quaternion.RotateTowards(_audience.headTransform.localRotation,
                                                                _defaultHeadLocalRotation,
                                                                SwitchFollowDegSpeed * Time.deltaTime);
            yield return null;
        }
        _audience.headTransform.localRotation = _defaultHeadLocalRotation;

        //2. gradually switch animator layer
        float initWeight = anim.GetLayerWeight(defaultLayerIdx);
        float timeElapsed = 0f;
        float totalTime = Mathf.Abs(1f - initWeight) / LerpAnimLayerSpeed;
        while (timeElapsed < totalTime)
        {
            anim.SetLayerWeight(defaultLayerIdx,
                Mathf.Lerp(initWeight, 1f, Mathf.Clamp01(timeElapsed / totalTime)));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        anim.SetLayerWeight(defaultLayerIdx, 1f);

        _currStopFollowCR = null;
    }

    //private void Update()
    //{
    //    //sample usage
    //    //if (Input.GetKeyDown(KeyCode.O))
    //    //    StartToFollow(GameObject.Find("Target").transform);
    //    //if (Input.GetKeyDown(KeyCode.P))
    //    //    StopToFollow();
    //}

    private void Awake()
    {
        _audience = GetComponent<Audience>();
        anim.SetLayerWeight(defaultLayerIdx, 1f);
    }

    private void Start()
    {
        StartCoroutine(Repeat_CR());
    }

    public void UpdateStateAnim()
    {
        
        anim.SetInteger(_paramIdState, (int)_audience.currState);
    }

    public void UpdateChatDirection(Vector2 dir)
    {
        anim.SetFloat(_paramIdFaceDirX, dir.x);
        anim.SetFloat(_paramIdFaceDirZ, dir.y);
    }

    private IEnumerator Repeat_CR()
    {
        while (true)
        {
            float repeatWaitTime = Mathf.Lerp(repeatPeriodBound.x, repeatPeriodBound.y, Random.value);
            yield return new WaitForSeconds(repeatWaitTime);
            if(Random.value > 0.5f)
                anim.SetTrigger(_paramIdSwitchPose);
        }
    }

    private IEnumerator LookAt_CR()
    {
        while (true)
        {
            _audience.headTransform.LookAt(_currTarget);
            yield return null;
        }
    }
}
