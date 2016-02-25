using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Reflection;

public class AreaTransitionData : MonoBehaviour
{
    public enum TransistionMoveType
    {
        Teleport, Walk, Parabola
    };

    public float preWaitTime;
    public float postWaitTime;

    public TransistionMoveType moveType;
    public float transitionSpeed;
    public float parabolaHeight;

    [Serializable]
    public class AreaTransitionEvent : UnityEvent<bool, bool> { }

    public AreaTransitionEvent OnPreTransition;
    public AreaTransitionEvent OnPostTransition;
}

