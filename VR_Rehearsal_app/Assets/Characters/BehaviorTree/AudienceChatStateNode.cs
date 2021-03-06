﻿using UnityEngine;
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
#if DEBUG
        if (target.socialGroup == null)
            Debug.LogError("null social group: " + target.agentId);
#endif
        Vector3 center = target.socialGroup.centerPos;
        Vector3 proj = Vector3.ProjectOnPlane((center - target.transform.position), target.transform.up);
        proj.Normalize();
        Vector2 chatDir = new Vector2
            (Vector3.Dot(proj, -target.transform.right),
              Vector3.Dot(proj, -target.transform.forward));
        float angle = 1 / Mathf.PI * Mathf.Atan2(chatDir.y, chatDir.x);
        if (angle < -0.5f)
            target.animHandler.UpdateChatDirection(-1f);
        else if (angle < 0f && angle >= -0.5f)
            target.animHandler.UpdateChatDirection(1f);
        else
            target.animHandler.UpdateChatDirection(Mathf.Lerp(1, -1, angle));
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