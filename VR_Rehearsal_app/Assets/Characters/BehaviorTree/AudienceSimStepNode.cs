using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MangoBehaviorTree;

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
        CrowdSimulator sim = Object.FindObjectOfType<CrowdSimulator>();
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
            target.stateMassFunction[(int)Audience.States.Focused] = 1f;
            target.stateMassFunction[(int)Audience.States.Bored] = 0f;
            target.stateMassFunction[(int)Audience.States.Chatting] = 0f;
        }
        else
        {
            target.stateMassFunction[(int)Audience.States.Focused] = global * pos;
            target.stateMassFunction[(int)Audience.States.Bored] = (1f - global) * (1f - pos);
            target.stateMassFunction[(int)Audience.States.Chatting] = (1f - global) * (1f - pos);
        }

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

    protected override void Close(Tick<Audience> tick)
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
