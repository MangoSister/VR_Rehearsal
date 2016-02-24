using UnityEngine;
using System.Collections;
using MangoBehaviorTree;
using System;
using State = Audience.States;
public class AudienceBoredStateNode : BaseNode<Audience>
{
    public AudienceBoredStateNode() : base() { }

    protected override void Open(Tick<Audience> tick)
    { return; }

    protected override void Enter(Tick<Audience> tick)
    { return; }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        tick.target.currState = State.Bored;
#if DEBUG
        /*
        Debug.Log(string.Format("{0} ({1}): {2}",
            tick.target.name, tick.target.agentId, _state.ToString()));
            */
#endif
        return NodeStatus.RUNNING;
    }

    protected override void Exit(Tick<Audience> tick)
    { return; }

    public override void Close(Tick<Audience> tick)
    { return; }
}