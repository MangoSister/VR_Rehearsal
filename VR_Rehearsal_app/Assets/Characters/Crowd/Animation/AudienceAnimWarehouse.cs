using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudienceAnimWarehouse : MonoBehaviour
{
    private static AudienceAnimWarehouse _curr;
    public static AudienceAnimWarehouse curr
    {
        get
        {
            if (_curr == null)
                _curr = FindObjectOfType<AudienceAnimWarehouse>();
            return _curr;
        }
    }

    public RuntimeAnimatorController basicController;
    public RuntimeAnimatorController followController;
    public RuntimeAnimatorController fullController;

    public GameObject laptopPrefab;
    public GameObject smartphonePrefab;

    [HideInInspector]
    public ProbabilitySingleClip[] basicFocusedClips;
    [HideInInspector]
    public ProbabilitySingleClip[] basicBoredClips;
    [HideInInspector]
    public ProbabilityClipPair[] basicChattingClips;
    [HideInInspector]
    public ProbabilitySingleClip[] followFocusedClips;
    [HideInInspector]
    public ProbabilitySingleClip[] followBoredClips;
    [HideInInspector]
    public ProbabilityClipPair[] followChattingClips;
}

[Serializable]
public class ProbabilitySingleClip
{
    public AnimationClip clip;

    [Range(0f, 1f)]
    public float probability;
}

[Serializable]
public class ProbabilityClipPair
{
    public AnimationClip clip1;
    public AnimationClip clip2;

    [Range(0f, 1f)]
    public float probability;
}
