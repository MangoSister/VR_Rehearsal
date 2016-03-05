using UnityEngine;
using System.Linq;

using MangoBehaviorTree;
using State = Audience.States;
using SimModule = CrowdSimulator.SimModule;

public class AudienceDefaultSimNode : BaseNode<Audience>
{
    protected override void Open(Tick<Audience> tick)
    {
        return;
    }

    protected override void Enter(Tick<Audience> tick)
    {
        return;
    }

    protected override void Exit(Tick<Audience> tick)
    {
        return;
    }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        CrowdSimulator sim = CrowdSimulator.currSim;
        Audience target = tick.target;

        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunction[i] = 1f / (float)target.stateMassFunction.Length;

        return NodeStatus.SUCCESS;
    }

    public override void Close(Tick<Audience> tick)
    {
        return;
    }
}
