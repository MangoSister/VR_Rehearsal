﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MangoBehaviorTree;

[RequireComponent(typeof(AudienceAnimHandler))]
public class Audience : MonoBehaviour, IAgent
{
    private AudienceAnimHandler _animHandler;

    public enum DetailLevel
    {
        Bump_FullAnim = 0,
        VL_FullAnim = 1,       
        VL_PoseAnim = 2,
        Billboard = 3,
    }

    public enum States
    {
        Focused = 0,
        Bored = 1,
        Chatting = 2,
    }

    [Range(0f, 1f)]
    public float attention = 1.0f;

    [Range(0f, 1f)]
    public float normalizedPos = 0.0f;

    [Range(0f, 1f)]
    public float gazeFactor = 0.0f;

    public float[] stateMassFunction;

    public DetailLevel detailLevel;

    [SerializeField]
    private States _currState;
    public States currState
    {
        get { return _currState; }
        set
        {
            _currState = value;

            _animHandler.UpdateStateAnim();

            if (_currState == States.Focused && followingTransform != null)
                _animHandler.StartToFollow(followingTransform);
            else _animHandler.StopToFollow();
        }
    }

    private int? _agentId;
    public int agentId
    {
        get
        {
            if (_agentId.HasValue)
                return _agentId.Value;
            else
            {
                _agentId = UniqueIdGenerator.Next;
                return _agentId.Value;
            }
        }
    }

    public Transform followingTransform;
    public Transform headTransform;

    private void Awake()
    {
        _animHandler = GetComponent<AudienceAnimHandler>();

        int num = Enum.GetNames(typeof(States)).Length;
        stateMassFunction = Enumerable.Repeat<float>(1f / (float)num, num).ToArray();
    }

}
