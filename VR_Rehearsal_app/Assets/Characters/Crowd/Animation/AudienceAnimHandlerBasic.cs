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
            //eyeIcon.transform.localScale = Vector3.one *
              //  eyeIconScale * (0.1f * Mathf.Sin(Time.time * eyeIconFreq) + 1f);
            yield return null;
        }

        eyeIcon.SetActive(false);
        _currEyeIconCR = null;
    }

    public override void UpdateStateAnim()
    {
        controller.SetInteger(_paramIdState, (int)_audience.currState);
    }

    public override void UpdateChatDirection(Vector2 dir)
    {
        controller.SetFloat(_paramIdBlendFactor0, dir.x);
        controller.SetFloat(_paramIdBlendFactor1, dir.y);
    }

    protected virtual void Awake()
    {
        _audience = GetComponent<Audience>();
        controller = GetComponentInChildren<Animator>();
        controller.runtimeAnimatorController = AudienceAnimClipHolder.curr.basicController;
        RandomizeBasicClips();
    }

    protected void RandomizeBasicClips()
    {
        var holder = AudienceAnimClipHolder.curr;
        if (holder == null)
            return;
        AnimatorOverrideController overCtrl = new AnimatorOverrideController();
        overCtrl.name = "Audience Override Anim Ctrl";
        overCtrl.runtimeAnimatorController = controller.runtimeAnimatorController;
        var clipPairs = overCtrl.clips;
        foreach (var pair in clipPairs)
        {
            if (holder.focusedClips != null && holder.focusedClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.focusedClips)
                {
                    if (pair.originalClip == clip)
                    {
                        pair.overrideClip = holder.focusedClips[Random.Range(0, holder.focusedClips.Length)];
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            if (holder.boredClips != null && holder.boredClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.boredClips)
                {
                    if (pair.originalClip == clip)
                    {
                        pair.overrideClip = holder.boredClips[Random.Range(0, holder.boredClips.Length)];
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            if (holder.chattingClips != null && holder.chattingClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.chattingClips)
                {
                    if (pair.originalClip == clip)
                    {
                        pair.overrideClip = holder.chattingClips[Random.Range(0, holder.chattingClips.Length)];
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }
        }

        overCtrl.clips = clipPairs;
        controller.runtimeAnimatorController = overCtrl;
    }
}
