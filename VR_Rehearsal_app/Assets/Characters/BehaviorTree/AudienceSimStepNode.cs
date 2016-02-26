using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MangoBehaviorTree;
using State = Audience.States;

public class AudienceSimStepNode : BaseNode<Audience>
{
    protected override void Open(Tick<Audience> tick)
    {
        return;
    }

    protected override void Enter(Tick<Audience> tick)
    {
        return;
    }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        Audience target = tick.target;
        
        float global = GaussianRandom(sim.globalAttentionMean, sim.globalAttentionStDev);
        global = Mathf.Clamp01(global);
        float pos = sim.seatPosAttentionFactor.Evaluate(target.normalizedPos);
        pos = Mathf.Clamp01(pos);

        target.gazeFactor = Mathf.Max(target.gazeFactor - sim.gazeCumulativeIntensity, 0f);
        target.gazeFactor += sim.gazeCollision.EvaluateGazePower(target.headTransform.position);
        target.gazeFactor = Mathf.Clamp01(target.gazeFactor);

        if (target.gazeFactor > 0.7f)
        {
            target.stateMassFunction[(int)State.Focused] = 1f;
            target.stateMassFunction[(int)State.Bored] = 0f;
            target.stateMassFunction[(int)State.Chatting] = 0f;
        }
        else
        {
            target.stateMassFunction[(int)State.Focused] = global * pos;
            target.stateMassFunction[(int)State.Bored] = (1f - global) * (1f - pos);
            target.stateMassFunction[(int)State.Chatting] = (1f - global) * (1f - pos);
        }

        if (target.socialGroup == null)
            target.stateMassFunction[(int)State.Chatting] = 0f;
        else if (!target.socialGroup.isComputed)
        {
            if (Random.value < target.stateMassFunction[(int)State.Chatting])
            {
                target.socialGroup.requestChat = true;
                target.stateMassFunction[(int)State.Focused] = 0f;
                target.stateMassFunction[(int)State.Bored] = 0f;
                target.stateMassFunction[(int)State.Chatting] = 1f;
            }
            else
            {
                target.socialGroup.requestChat = false;
                target.stateMassFunction[(int)State.Chatting] = 0f;
            }
            target.socialGroup.isComputed = true;

        }
        else if (target.socialGroup.isComputed && target.socialGroup.requestChat)
        {
            target.stateMassFunction[(int)State.Focused] = 0f;
            target.stateMassFunction[(int)State.Bored] = 0f;
            target.stateMassFunction[(int)State.Chatting] = 1f;
        }
        else target.stateMassFunction[(int)State.Chatting] = 0f;


        float sum = 0f;
        foreach (var mass in target.stateMassFunction)
            sum += mass;
        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunction[i] /= sum;

        return NodeStatus.SUCCESS;
    }

    protected override void Exit(Tick<Audience> tick)
    {
        return;
    }

    public override void Close(Tick<Audience> tick)
    {
        return;
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
