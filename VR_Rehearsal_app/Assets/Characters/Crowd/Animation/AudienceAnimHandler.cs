/* AudienceAnimHandler.cs
 * Yang Zhou, last modified on Feb 09, 2016
 * AudienceAnimHandler updates character animation as well as targeting behavior
 * Dependencies: character animator, and Audience.cs
 */

using UnityEngine;
using System.Collections;

public class AudienceAnimHandler : MonoBehaviour
{
    public readonly static int _paramIdState = Animator.StringToHash("state");
    public readonly static int _paramIdBlendFactor0 = Animator.StringToHash("blendFactor0");
    public readonly static int _paramIdBlendFactor1 = Animator.StringToHash("blendFactor1");
    public readonly static int _paramIdSubState = Animator.StringToHash("subState");
    public readonly static int _paramIdMirror = Animator.StringToHash("mirror");

    private Audience _audience;
    public Animator controller;

    //Random period to switch animation for variation
    public Vector2 repeatPeriodBound = new Vector2(3f, 8f);

    //Layer lerping speed during following transition
    public float LerpAnimLayerSpeed = 1.0f;
    
    //Neck rotatin angular speed during following transition
    public float SwitchFollowDegSpeed = 60f;

    public static Vector3 eyeIconOffset = Vector3.forward * 0.2f + Vector3.up * 0.1f;
    public static float eyeIconScale = 0.05f;
    public static float eyeIconFreq = 6f;
    public GameObject eyeIcon;
    public bool eyeIconToggle
    {
        get { return eyeIcon != null && eyeIcon.activeSelf; }
        set
        {
            if (eyeIcon != null)
            {
                eyeIcon.SetActive(value);
                if (value == true)
                {
                    _currEyeIconCR = StartCoroutine(IconAnimation_CR());
                }
                else
                {
                    if (_currEyeIconCR != null)
                    {
                        StopCoroutine(_currEyeIconCR);
                        _currEyeIconCR = null;
                    }
                }
            }
        }
    }
    private Coroutine _currEyeIconCR = null;
    private Transform _currTarget;
    //private bool _isFollowing = false;
    private Quaternion _defaultHeadLocalRotation;

    private Coroutine _currStartFollowCR = null;
    private Coroutine _currStopFollowCR = null;
    private Coroutine _currLookAtCR = null;

    //Animator layer index, must be consistent with the actual animator
    private const int neckConstraintLayerIdx = 0;
    private const int defaultLayerIdx = 1;
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
        _currStopFollowCR = StartCoroutine(StopToFollow_CR());
    }

    private IEnumerator StartToFollow_CR(Transform target)
    {
        //1. gradually switch animator layer
        float initWeight = controller.GetLayerWeight(defaultLayerIdx);
        float timeElapsed = 0f;
        float totalTime = Mathf.Abs(0f - initWeight) / LerpAnimLayerSpeed;
        while (timeElapsed < totalTime)
        {
            controller.SetLayerWeight(defaultLayerIdx,
                Mathf.Lerp(initWeight, 0f, Mathf.Clamp01(timeElapsed / totalTime)));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        controller.SetLayerWeight(defaultLayerIdx, 0f);

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
        float initWeight = controller.GetLayerWeight(defaultLayerIdx);
        float timeElapsed = 0f;
        float totalTime = Mathf.Abs(1f - initWeight) / LerpAnimLayerSpeed;
        while (timeElapsed < totalTime)
        {
            controller.SetLayerWeight(defaultLayerIdx,
                Mathf.Lerp(initWeight, 1f, Mathf.Clamp01(timeElapsed / totalTime)));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        controller.SetLayerWeight(defaultLayerIdx, 1f);

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
        controller.SetLayerWeight(defaultLayerIdx, 1f);
    }

    public void UpdateStateAnim()
    {
        controller.SetInteger(_paramIdState, (int)_audience.currState);
    }

    public void UpdateChatDirection(Vector2 dir)
    {
        controller.SetFloat(_paramIdBlendFactor1, dir.y);
    }

    private IEnumerator LookAt_CR()
    {
        while (true)
        {
            _audience.headTransform.LookAt(_currTarget);
            yield return null;
        }
    }

    private IEnumerator IconAnimation_CR()
    {
        while (true)
        {
            eyeIcon.transform.localScale = Vector3.one * 
                eyeIconScale * (0.1f * Mathf.Sin(Time.time * eyeIconFreq) + 1f);
            yield return null;
        }
    }
}
