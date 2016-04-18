using UnityEngine;
using System.Collections;

public class AudienceAnimHandlerBasic : AudienceAnimHandlerAbstract
{

    protected Audience _audience;
    public Animator controller;


    public GameObject eyeIcon;
    private Coroutine _currEyeIconCR = null;
    private bool _eyeIconToggle = false;
    public bool eyeIconToggle
    {
        get { return _eyeIconToggle; }
        set
        {
            if (eyeIcon != null)
            {
                if (value == true)
                {
                    if (!_eyeIconToggle)
                    {
                        _eyeIconToggle = value;
                        eyeIcon.SetActive(value);
                        if (_currEyeIconCR == null)
                            _currEyeIconCR = StartCoroutine(IconAnimation_CR());
                    }
                }
                else
                {
                    _eyeIconToggle = value;
                    eyeIcon.SetActive(false);
                    if (_currEyeIconCR != null)
                    {
                        StopCoroutine(_currEyeIconCR);
                        _currEyeIconCR = null;
                    }
                }
            }
        }
    }

    public bool isManualCtrl = false;

    private IEnumerator IconAnimation_CR()
    {
        float initScale = 0.8f;
        float currTime = 0f, totalTime = 1f / eyeIconFreq;
        while (currTime < totalTime)
        {
            float currScale = Mathf.Lerp(initScale, 1f, currTime * eyeIconFreq);
            eyeIcon.transform.localScale = Vector3.one * eyeIconScale * currScale;
            currTime += Time.deltaTime;
            yield return null;
        }

        eyeIcon.SetActive(false);
        _currEyeIconCR = null;
    }

    public override void UpdateStateAnim()
    {
        controller.SetInteger(_paramIdState, (int)_audience.currState);
    }

    public override void UpdateChatDirection(float dir)
    {
        controller.SetFloat(_paramIdBlendFactor1, dir);
    }

    protected virtual void Awake()
    {
        _audience = GetComponent<Audience>();
        controller = GetComponentInChildren<Animator>();
        controller.runtimeAnimatorController = AudienceAnimWarehouse.curr.basicController;
        RandomizeBasicClips();
    }

    private void RandomizeBasicClips()
    {
        var holder = AudienceAnimWarehouse.curr;
        if (holder == null)
            return;
        AnimatorOverrideController overCtrl = new AnimatorOverrideController();
        overCtrl.name = "Basic Audience Override Anim Ctrl";
        overCtrl.runtimeAnimatorController = controller.runtimeAnimatorController;
        var clipPairs = overCtrl.clips;

        AnimationClipPair leftChatPair = null;
        AnimationClipPair rightChatPair = null;

        foreach (var pair in clipPairs)
        {
            if (holder.basicFocusedClips != null && holder.basicFocusedClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.basicFocusedClips)
                {
                    if (pair.originalClip == clip.clip)
                    {
                        float sample = Random.value;
                        for (int i = 0; i < holder.basicFocusedClips.Length; ++i)
                            if (sample < holder.basicFocusedClips[i].probability)
                            {
                                pair.overrideClip = holder.basicFocusedClips[i].clip;
                                break;
                            }
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            if (holder.basicBoredClips != null && holder.basicBoredClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.basicBoredClips)
                {
                    if (pair.originalClip == clip.clip)
                    {
                        float sample = Random.value;
                        for (int i = 0; i < holder.basicBoredClips.Length; ++i)
                            if (sample < holder.basicBoredClips[i].probability)
                            {
                                pair.overrideClip = holder.basicBoredClips[i].clip;
                                break;
                            } 
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            if (holder.basicChattingClips != null && holder.basicChattingClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.basicChattingClips)
                {
                    if (pair.originalClip == clip.clip1)
                    {
                        leftChatPair = pair;
                        break;
                    }
                    else if (pair.originalClip == clip.clip2)
                    {
                        rightChatPair = pair;
                        break;
                    }
                }
                if (found)
                    continue;
            }
        }

        float s = Random.value;
        for (int i = 0; i < holder.basicChattingClips.Length; ++i)
            if (s < holder.basicChattingClips[i].probability)
            {
                leftChatPair.overrideClip = holder.basicChattingClips[i].clip1;
                rightChatPair.overrideClip = holder.basicChattingClips[i].clip2;
                break;
            }

        overCtrl.clips = clipPairs;
        controller.runtimeAnimatorController = overCtrl;
    }
}
