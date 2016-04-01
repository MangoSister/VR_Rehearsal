using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using States = Audience.States;

public class AudienceAnimHandlerFull : AudienceAnimHandlerFollow
{
    public static readonly Dictionary<States, int> subStateNum = new Dictionary<States, int>()
    {
        {States.Focused, 3 },
        {States.Bored, 5 },
        {States.Chatting, 1 }
    };

    public Vector2 repeatPeriodBound = new Vector2(3f, 8f);

    public override void UpdateStateAnim()
    {
        base.UpdateStateAnim();
        int nextSubState = Random.Range(0, subStateNum[_audience.currState]);
        controller.SetInteger(_paramIdSubState, nextSubState);
        float nextBlendFactor0 = Random.value;
        controller.SetFloat(_paramIdBlendFactor0, nextBlendFactor0);
        bool nextMirror = Random.value > 0.5f;
        controller.SetBool(_paramIdMirror, nextMirror);
    }

    public override void UpdateChatDirection(Vector2 dir)
    {
        base.UpdateChatDirection(dir);
        if (Mathf.Abs(dir.y) < 0.5f)
            controller.SetFloat(_paramIdBlendFactor0, 0f);
    }

    private IEnumerator SwitchSubState_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Lerp(repeatPeriodBound.x, repeatPeriodBound.y, Random.value));
            int nextSubState = Random.Range(0, subStateNum[_audience.currState]);
            controller.SetInteger(_paramIdSubState, nextSubState);
        }
    }

    protected override void Awake()
    {
        _audience = GetComponent<Audience>();
        controller = GetComponentInChildren<Animator>();
        controller.runtimeAnimatorController = AudienceAnimClipHolder.curr.fullController;
        controller.SetLayerWeight(defaultLayerIdx, 1f);
    }
}
