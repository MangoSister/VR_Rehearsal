using UnityEngine;
using System.Linq;

using MangoBehaviorTree;
using State = Audience.States;
using SimModule = CrowdSimulator.SimModule;

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

        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunction[i] = 1f / (float)target.stateMassFunction.Length;

        ProcessGlobal(target);
        ProcessSeatDistribution(target);
        ProcessSocialGroup(target); //override

        ProcessFillerWord(target);
        ProcessVoiceVolume(target);
        ProcessGaze(target); //override

        float sum = 0f;
        foreach (var mass in target.stateMassFunction)
            sum += mass;
        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunction[i] /= sum;

        if (sim.deterministic)
        {
            float max = target.stateMassFunction.Max();
            for (int i = 0; i < target.stateMassFunction.Length; ++i)
            {
                if (target.stateMassFunction[i] == max)
                    target.stateMassFunction[i] = 1f;
                else target.stateMassFunction[i] = 0f;
            }
        }

        return NodeStatus.SUCCESS;
    }

    private void ProcessGaze(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.Gaze) == 0x00)
            return;

        target.gazeFactor = Mathf.Max(target.gazeFactor - sim.gazeCumulativeIntensity, 0f);
        target.gazeFactor += sim.gazeCollision.EvaluateGazePower(target.headTransform.position);
        target.gazeFactor = Mathf.Clamp01(target.gazeFactor);

        if (target.gazeFactor > 0.7f)
        {
            target.stateMassFunction[(int)State.Focused] = 1f;
            target.stateMassFunction[(int)State.Bored] = 0f;
            target.stateMassFunction[(int)State.Chatting] = 0f;
        }
    }

    private void ProcessVoiceVolume(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.VoiceVolume) == 0x00)
            return;
    }

    private void ProcessFillerWord(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.FillerWords) == 0x00)
            return;
    }

    private void ProcessSeatDistribution(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.SeatDistribution) == 0x00)
            return;

        float pos = sim.seatPosAttentionFactor.Evaluate(target.normalizedPos);
        pos = Mathf.Clamp(pos, -1f, 1f);
        target.stateMassFunction[(int)State.Focused] += pos;
        target.stateMassFunction[(int)State.Bored] -= pos;
        target.stateMassFunction[(int)State.Chatting] -= pos;

        if (target.stateMassFunction[(int)State.Focused] < 0f)
            target.stateMassFunction[(int)State.Focused] = 0f;
        if (target.stateMassFunction[(int)State.Bored] < 0f)
            target.stateMassFunction[(int)State.Bored] = 0f;
        if (target.stateMassFunction[(int)State.Chatting] < 0f)
            target.stateMassFunction[(int)State.Chatting] = 0f;
    }

    private void ProcessSocialGroup(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.SocialGroup) == 0x00)
        {
            target.stateMassFunction[(int)State.Chatting] = 0f;
            return;
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

    }

    private void ProcessGlobal(Audience target)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        if ((sim.simModule & SimModule.Global) == 0x00)
            return;

        float global = GaussianRandom(sim.globalAttentionMean, sim.globalAttentionStDev);
        global = Mathf.Clamp(global, -1f, 1f);
        target.stateMassFunction[(int)State.Focused] += global;
        target.stateMassFunction[(int)State.Bored] -= global;
        target.stateMassFunction[(int)State.Chatting] -= global;

        if (target.stateMassFunction[(int)State.Focused] < 0f)
            target.stateMassFunction[(int)State.Focused] = 0f;
        if (target.stateMassFunction[(int)State.Bored] < 0f)
            target.stateMassFunction[(int)State.Bored] = 0f;
        if (target.stateMassFunction[(int)State.Chatting] < 0f)
            target.stateMassFunction[(int)State.Chatting] = 0f;

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
