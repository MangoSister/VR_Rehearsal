using System.Linq;
using UnityEngine;
using MangoBehaviorTree;
using State = Audience.States;
using SimModule = CrowdSimulator.SimModule;

public class AudienceInternalSimNode : BaseNode<Audience>
{
    protected override void Enter(Tick<Audience> tick)
    {
        return;
    }

    protected override void Open(Tick<Audience> tick)
    {
        return;
    }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
#if UNITY_EDITOR
        Debug.Log("internal sim");
#endif
        Audience target = tick.target;
        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunctionInternal[i] = 1f / target.stateMassFunction.Length;

        ProcessGlobal(target);
        ProcessSeatDistribution(target);
        ProcessSocialGroup(target); //override

        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunction[i] = target.stateMassFunctionInternal[i];

        target.inertiaLock = true;

        return NodeStatus.SUCCESS;
    }

    public override void Close(Tick<Audience> tick)
    {
        return;
    }

    protected override void Exit(Tick<Audience> tick)
    {
        return;
    }

    private void ProcessSeatDistribution(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.SeatDistribution) == 0x00)
            return;

        float pos = sim.seatPosAttentionFactor.Evaluate(target.normalizedPos);
        pos = Mathf.Clamp(pos, -1f, 1f);
        target.stateMassFunctionInternal[(int)State.Focused] += pos;
        target.stateMassFunctionInternal[(int)State.Bored] -= pos;
        target.stateMassFunctionInternal[(int)State.Chatting] -= pos;

        if (target.stateMassFunctionInternal[(int)State.Focused] < 0f)
            target.stateMassFunctionInternal[(int)State.Focused] = 0f;
        if (target.stateMassFunctionInternal[(int)State.Bored] < 0f)
            target.stateMassFunctionInternal[(int)State.Bored] = 0f;
        if (target.stateMassFunctionInternal[(int)State.Chatting] < 0f)
            target.stateMassFunctionInternal[(int)State.Chatting] = 0f;
    }

    private void ProcessSocialGroup(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.SocialGroup) == 0x00)
        {
            target.stateMassFunctionInternal[(int)State.Chatting] = 0f;
            return;
        }

        if (target.socialGroup == null)
            target.stateMassFunctionInternal[(int)State.Chatting] = 0f;
        else if (!target.socialGroup.isComputed)
        {
            if (Random.value < target.stateMassFunctionInternal[(int)State.Chatting])
            {
                target.socialGroup.requestChat = true;
                target.stateMassFunctionInternal[(int)State.Focused] = 0f;
                target.stateMassFunctionInternal[(int)State.Bored] = 0f;
                target.stateMassFunctionInternal[(int)State.Chatting] = 1f;
            }
            else
            {
                target.socialGroup.requestChat = false;
                target.stateMassFunctionInternal[(int)State.Chatting] = 0f;
            }
            target.socialGroup.isComputed = true;

        }
        else if (target.socialGroup.isComputed && target.socialGroup.requestChat)
        {
            target.stateMassFunctionInternal[(int)State.Focused] = 0f;
            target.stateMassFunctionInternal[(int)State.Bored] = 0f;
            target.stateMassFunctionInternal[(int)State.Chatting] = 1f;
        }
        else target.stateMassFunctionInternal[(int)State.Chatting] = 0f;

    }

    private void ProcessGlobal(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.Global) == 0x00)
            return;

        float global = sim.globalAttentionAmp * GaussianRandom(sim.globalAttentionMean, sim.globalAttentionStDev) +
                        sim.globalAttentionConstOffset;
        global = Mathf.Clamp(global, -1f, 1f);
        target.stateMassFunctionInternal[(int)State.Focused] += global;
        target.stateMassFunctionInternal[(int)State.Bored] -= global;
        target.stateMassFunctionInternal[(int)State.Chatting] -= global;

        if (target.stateMassFunctionInternal[(int)State.Focused] < 0f)
            target.stateMassFunctionInternal[(int)State.Focused] = 0f;
        if (target.stateMassFunctionInternal[(int)State.Bored] < 0f)
            target.stateMassFunctionInternal[(int)State.Bored] = 0f;
        if (target.stateMassFunctionInternal[(int)State.Chatting] < 0f)
            target.stateMassFunctionInternal[(int)State.Chatting] = 0f;

    }

    private float GaussianRandom(float mean, float stdDev)
    {
        float u1 = Random.value;
        float u2 = Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                     Mathf.Sin(2.0f * Mathf.PI * u2); //random normal(0,1)
        float randNormal = mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }
}

