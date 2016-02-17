using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MangoBehaviorTree;

[RequireComponent(typeof(AudienceAnimHandler))]
public class Audience : MonoBehaviour, IAgent
{
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
    private States _currState;
    public States currState
    {
        get { return _currState; }
        set
        {
            if (value != _currState && OnStateChange != null)
                OnStateChange();
            _currState = value;
        }
    }

    public delegate void StateChange_Handler();
    public StateChange_Handler OnStateChange;

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

    public Transform headTransform;

    private void Awake()
    {
        int num = Enum.GetNames(typeof(States)).Length;
        stateMassFunction = Enumerable.Repeat<float>(1f / (float)num, num).ToArray();
        if (OnStateChange == null)
            OnStateChange += GetComponent<AudienceAnimHandler>().UpdateStateAnim;
    }

}
