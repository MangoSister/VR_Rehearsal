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
        controller.runtimeAnimatorController = AudienceAnimWarehouse.curr.basicController;
        RandomizeBasicClips();
    }

    private void RandomizeBasicClips()
    {
        var holder = AudienceAnimWarehouse.curr;
        if (holder == null)
            return;
        AnimatorOverrideController overCtrl = new AnimatorOverrideController();
        overCtrl.name = "Audience Override Anim Ctrl";
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
                    if (pair.originalClip == clip)
                    {
                        pair.overrideClip = holder.basicFocusedClips[Random.Range(0, holder.basicFocusedClips.Length)];
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
                    if (pair.originalClip == clip)
                    {
                        pair.overrideClip = holder.basicBoredClips[Random.Range(0, holder.basicBoredClips.Length)];
                        found = true;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            if (holder.basicLeftChattingClips != null && holder.basicLeftChattingClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.basicLeftChattingClips)
                {
                    if (pair.originalClip == clip)
                    {
                        leftChatPair = pair;
                        break;
                    }
                }
                if (found)
                    continue;
            }

            if (holder.basicRightChattingClips != null && holder.basicRightChattingClips.Length > 0)
            {
                bool found = false;
                foreach (var clip in holder.basicRightChattingClips)
                {
                    if (pair.originalClip == clip)
                    {
                        rightChatPair = pair;
                        break;
                    }
                }
                if (found)
                    continue;
            }
        }

        int chatClipIdx = Random.Range(0, holder.basicLeftChattingClips.Length);
        leftChatPair.overrideClip = holder.basicLeftChattingClips[chatClipIdx];
        rightChatPair.overrideClip = holder.basicRightChattingClips[chatClipIdx];

        overCtrl.clips = clipPairs;
        controller.runtimeAnimatorController = overCtrl;
    }
}
