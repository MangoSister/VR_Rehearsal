using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MangoBehaviorTree;

public class AudienceStateSelectorNode : RandomSelectorNode<Audience>
{
    public AudienceStateSelectorNode(int sampleTime, List<BaseNode<Audience>> children)
        : base(sampleTime, children) { }

    public AudienceStateSelectorNode(int sampleTime, params BaseNode<Audience>[] children)
        : base(sampleTime, children) { }

    protected override void UpdateWeights(Tick<Audience> tick)
    {
        var info = _agentExecInfo[tick.target.agentId];
        info.weights = tick.target.stateMassFunction;
        float sum = 0f;
        foreach (var weight in info.weights)
            sum += weight;
        for (int i = 0; i < info.weights.Length; ++i)
            info.weights[i] /= sum;

        _agentExecInfo[tick.target.agentId] = info;

    }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        if (tick.target.updateLock)
        {
            tick.target.updateLock = false;
            return base.Tick(tick);
        }
        else return children[(int)tick.target.currState].Execute(tick);
    }
}
