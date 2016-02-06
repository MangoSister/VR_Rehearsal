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
        Debug.Log("tick state: " + _state.ToString());
        return NodeStatus.SUCCESS;
    }

    protected override void Exit(Tick<Audience> tick)
    { return; }

    protected override void Close(Tick<Audience> tick)
    { return; }
}