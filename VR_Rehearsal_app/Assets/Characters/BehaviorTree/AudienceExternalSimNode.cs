using UnityEngine;
using System.Linq;

using MangoBehaviorTree;
using State = Audience.States;
using SimModule = CrowdSimulator.SimModule;


public class AudienceExternalSimNode : BaseNode<Audience>
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
//#if UNITY_EDITOR
//        Debug.Log("external sim");
//#endif
        CrowdSimulator sim = CrowdSimulator.currSim;
        Audience target = tick.target;

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
            bool unique = false;
            for (int i = 0; i < target.stateMassFunction.Length; ++i)
            {
                if (target.stateMassFunction[i] == max && !unique)
                {
                    target.stateMassFunction[i] = 1f;
                    unique = true;
                }
                else target.stateMassFunction[i] = 0f;
            }
        }

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
            target.inertiaLock = true;
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
}

