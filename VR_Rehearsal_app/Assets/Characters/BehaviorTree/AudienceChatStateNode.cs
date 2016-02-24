using UnityEngine;
using System.Collections;
using MangoBehaviorTree;
using System;
using State = Audience.States;
using DetailLevel = Audience.DetailLevel;

public class AudienceChatStateNode : BaseNode<Audience>
{
    public AudienceChatStateNode() : base() { }

    protected override void Open(Tick<Audience> tick)
    {
        Audience target = tick.target;
        //Vector3 center = target.socialGroup.centerPos;
        //Vector3 proj = Vector3.ProjectOnPlane((target.transform.position - center), target.transform.up);
        //target.animHandler.UpdateChatDirection(new Vector2
        //    (Vector3.Dot(proj, target.transform.right),
        //      Vector3.Dot(proj, target.transform.forward)));
    }

    protected override void Enter(Tick<Audience> tick)
    { return; }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        tick.target.currState = State.Chatting;
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
    {

    }
}