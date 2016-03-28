/* Audience.cs
 * Yang Zhou, last modified on Mar 27, 2016
 * Audience represents a member of virtual audience
 * Dependencies: AudienceAnimHandler.cs, behavior tree system, gaze detector, recordwrapper, crowd simulator
 */

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

    /*
    LOD system: Different audience prefab sets for crowd simulator
    */
    public enum DetailLevel
    {
        //lights are all probed
        FullSize_Bump_FullAnim = 0, //whole body mesh, normal mapped, all animations (including script driven ones) 
        FullSize_FullAnim = 1, //whole body mesh, only diffuse, all animations (including script driven ones)
        HalfSize_FullAnim = 2, //half body mesh, only diffuse, all animations (but no script driven ones)
        Billboard = 3, //2D billboard, no animations
    }

    /*
    Discretized behavior states, the three states are the highest-leveled states
    In actual animation system, there are subleveled states
    */
    public enum States
    {
        Focused = 0,
        Bored = 1,
        Chatting = 2,
    }

    /* 
    The normalized position (relative to the position of the speaker) 
    0: near / 1: far
    */
    [Range(0f, 1f)]
    public float normalizedPos = 0.0f;

    /* 
    The influence caused by gaze
    */
    [Range(0f, 1f)]
    public float gazeFactor = 0.0f;

    /*
        According to the two pass process of behavior tree,
        there are two probability distribution
        [1/3, 1/3, 1/3] are the initial value of every simulation step
        stateMassFunctionInternal is the result of the first pass
        stateMassFunction gets stateMassFunctionInternal as its initial value, 
        and gets modified by the second pass.
    */
    public float[] stateMassFunctionInternal;
    public float[] stateMassFunction;

    public DetailLevel detailLevel;

    /*
        Current state.
        Only update animation when current state actually is changed.
    */
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

    /*
        unique id for each agent
    */
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

    /* convenient reference, the target of the agent's following behavior (used by animation handler)  */
    public Transform followingTransform;

    /* convenient reference */
    public Transform headTransform;

    /* a reference to its social group */
    public SocialGroup socialGroup = null;

    /* lazy update strategy of behavior tree system */
    public bool lazyUpdateLock = false;

    /* shuffle the internal update time so audience update uniformly during an interval (no "update spike") */
    public float simInternalOffset = 0f;

    /* attention is the weight of focused state */
    public float attention
    { get { return stateMassFunction[(int)States.Focused]; } }

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
