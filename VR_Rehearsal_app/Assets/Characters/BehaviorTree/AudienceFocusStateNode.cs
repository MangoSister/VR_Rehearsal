using UnityEngine;
using System.Collections;
using MangoBehaviorTree;
using System;
using State = Audience.States;
using DetailLevel = Audience.DetailLevel;

public class AudienceFocusStateNode : BaseNode<Audience>
{
    public AudienceFocusStateNode() : base()  { }

    protected override void Open(Tick<Audience> tick)
    {
        Audience target = tick.target;
        if (target.detailLevel == DetailLevel.FullSize_Bump_FullAnim ||
            target.detailLevel == DetailLevel.FullSize_VL_FullAnim)
            target.animHandler.StartToFollow(target.followingTransform);
    }

    protected override void Enter(Tick<Audience> tick)
    { return; }

    protected override NodeStatus Tick(Tick<Audience> tick)
    {
        tick.target.currState = State.Focused;
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
        Audience target = tick.target;
        if (target.detailLevel == DetailLevel.FullSize_Bump_FullAnim ||
            target.detailLevel == DetailLevel.FullSize_VL_FullAnim)
            target.animHandler.StopToFollow();
    }
}