using UnityEngine;
using MangoBehaviorTree;

public class AudienceBypassInternalNode : BaseNode<Audience>
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
//        Debug.Log("bypass internal");
//#endif
        Audience target = tick.target;
        for (int i = 0; i < target.stateMassFunction.Length; ++i)
            target.stateMassFunction[i] = target.stateMassFunctionInternal[i];
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
}
