using UnityEngine;
using System.Collections;
using MangoBehaviorTree;
using System;
using State = Audience.States;
public class AudienceStateNode : BaseNode<Audience>
{
    private State _state;

    public AudienceStateNode(State state) : base()
    {
        _state = state;
    }

    protected override void Open(Tick<Audience> tick)
    { return; }

    protected override void Enter(Tick<Audience> tick)
    { return; }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        tick.target.currState = _state;
#if DEBUG
        /*
        Debug.Log(string.Format("{0} ({1}): {2}",
            tick.target.name, tick.target.agentId, _state.ToString()));
            */
#endif
        return NodeStatus.SUCCESS;
    }

    protected override void Exit(Tick<Audience> tick)
    { return; }

    protected override void Close(Tick<Audience> tick)
    { return; }
}