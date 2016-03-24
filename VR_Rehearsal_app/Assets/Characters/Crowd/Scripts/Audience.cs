using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MangoBehaviorTree;

[RequireComponent(typeof(AudienceAnimHandler))]
public class Audience : MonoBehaviour, IAgent
{
    private AudienceAnimHandler _animHandler;
    public AudienceAnimHandler animHandler { get { return _animHandler; } }

    public enum DetailLevel
    {
        FullSize_Bump_FullAnim = 0,
        FullSize_VL_FullAnim = 1,       
        HalfSize_VL_FullAnim = 2,
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

    public float[] stateMassFunctionInternal;
    public float[] stateMassFunction;

    public DetailLevel detailLevel;

    [SerializeField]
    private States _currState;
    public States currState
    {
        get { return _currState; }
        set
        {
            if (_currState != value)
            {
                _currState = value;
                _animHandler.UpdateStateAnim();
            }
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
                _agentId = NextGlobalId;
                return _agentId.Value;
            }
        }
    }

    public Transform followingTransform;
    public Transform headTransform;

    public SocialGroup socialGroup = null;

    public bool inertiaLock = false;
    public float simInternalOffset = 0f;
    private void Awake()
    {
        _animHandler = GetComponent<AudienceAnimHandler>();

        int num = Enum.GetNames(typeof(States)).Length;
        stateMassFunctionInternal = Enumerable.Repeat<float>(1f / (float)num, num).ToArray();
        stateMassFunction = Enumerable.Repeat<float>(1f / (float)num, num).ToArray();
    }


    private static int _globalId = 0;
    public static int NextGlobalId
    {
        get
        {
            if (_globalId == int.MaxValue)
                throw new Exception("cannot assign id anymore");
            return _globalId++;
        }
    }
}
