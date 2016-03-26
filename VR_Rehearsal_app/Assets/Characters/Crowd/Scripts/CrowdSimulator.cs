﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MangoBehaviorTree;
using CrowdConfigInfo = spaceInfoParser.parsedData_spaceInfo;
using URandom = UnityEngine.Random;

public class CrowdSimulator : MonoBehaviour
{
    [Flags]
    public enum SimModule
    {
        Gaze = 1,
        VoiceVolume = 2,
        FillerWords = 4,
        SeatDistribution = 8,
        SocialGroup = 16,
        Global = 32,
    };

    private static CrowdSimulator _currSim = null;
    public static CrowdSimulator currSim
    {
        get
        {
            if (_currSim == null)
                _currSim = FindObjectOfType<CrowdSimulator>();
            return _currSim;
        }
    }

    public Audience[] prefabsL0;
    public Audience[] prefabsL1;
    public Audience[] prefabsL2;
    public Audience[] prefabsL3;

    public Transform crowdParent;
    public string crowdConfigFileName;
    public float stepIntervalInt;
    public float stepIntervalExt;
    public float stepIntervalInput;

    public SimModule simModule = 
        SimModule.Gaze | SimModule.VoiceVolume | 
        SimModule.FillerWords | SimModule.SeatDistribution | 
        SimModule.SocialGroup | SimModule.Global;

    public bool deterministic;

    public float globalAttentionMean;
    public float globalAttentionStDev;
    public float globalAttentionAmp;
    public float globalAttentionConstOffset;

    public AnimationCurve seatPosAttentionCurve;
    public float seatPosAttentionUpper
    {
        get
        {
            return seatPosAttentionCurve.keys[0].value;
        }
        set
        {
            var keys = seatPosAttentionCurve.keys;
            keys[0].value = value;
            seatPosAttentionCurve = new AnimationCurve(keys);

        }
    }
    public float seatPosAttentionLower
    {
        get
        {
            return seatPosAttentionCurve.keys[1].value;
        }
        set
        {
            var keys = seatPosAttentionCurve.keys;
            keys[1].value = value;
            seatPosAttentionCurve = new AnimationCurve(keys);
        }
    }

    public SimpleGazeCollision gazeCollision;
    public RecordingWrapper recordWrapper;

    [Range(0f,1f)]
    public float gazeCumulativeIntensity;

    public float voiceUpdatePeriod;
    public AnimationCurve fluencyCurve;

    private List<Audience> audiences;
    public int audienceNum { get { return audiences.Count; } }
    private List<SocialGroup> socialGroups;
    public float noChatThreshold;
    public float avgChatThreshold;
    public float chatLength;
    public float genChatPeriod;

    //private BehaviorTree<Audience> _audienceBt = null;
    private BehaviorTree<Audience> _behaviorTree;

    private void CreateBehaviorTree()
    {
        _behaviorTree = new BehaviorTree<Audience>
            (new SequenceNode<Audience>
                (new SelectorNode<Audience>
                    (new SequenceNode<Audience>
                        (new InstantSuccessModifier<Audience>(new WaitNode<Audience>(stepIntervalInt)),
                        new AudienceInternalSimNode()),
                    new AudienceBypassInternalNode()),
                new AudienceExternalSimNode(),
                new AudienceStateSelectorNode
                    (1, 
                    new AudienceFocusStateNode(), 
                    new AudienceBoredStateNode(), 
                    new AudienceChatStateNode())));
    }

    private void CreateCrowd()
    {
        //instantiate audience
        CrowdConfigInfo tx = spaceInfoParser.Parse(crowdConfigFileName);
        audiences = new List<Audience>();

        for (int i = 0; i < tx.seat_RowNum * tx.seat_ColNum; i++)
        {
            int rand = URandom.Range(0, (prefabsL1.Length - 1));
            Audience ad;
            if (i % tx.seat_ColNum < 2)
            {
                ad = Instantiate(prefabsL1[rand], Vector3.zero, Quaternion.identity) as Audience;
                ad.detailLevel = Audience.DetailLevel.FullSize_VL_FullAnim;
            }
            else
            {
                ad = Instantiate(prefabsL2[rand], Vector3.zero, Quaternion.identity) as Audience;
                ad.detailLevel = Audience.DetailLevel.HalfSize_VL_FullAnim;
            }
            ad.simInternalOffset = URandom.Range(0, stepIntervalInt);
            ad.followingTransform = RoomCenter.currRoom.presenterHead;
            ad.GetComponent<AudienceAnimHandler>().repeatPeriodBound = new Vector2(10000f, 400000f);

            //to Phan: fix the layout here
            ad.normalizedPos = (float)(i % tx.seat_ColNum) / (float)tx.seat_ColNum;
            ad.transform.parent = crowdParent;
            ad.transform.localPosition = tx.seat_posVecs[i];
            audiences.Add(ad);
        }

        //create social group
        socialGroups = new List<SocialGroup>();
        for (int i = 0; i < audiences.Count; i++)
        {
            if (URandom.value > 0.3f)
                continue;

            int z = i % tx.seat_ColNum;
            int x = i / tx.seat_ColNum;
            int idx = z + x * tx.seat_ColNum;

            if (audiences[idx].socialGroup != null)
                continue;

            List<Audience> neighbors = new List<Audience>();
            //randomly create social groups for now, 8 connectivity neighbors
            if (idx + 1 >= 0 && idx + 1 < audiences.Count && audiences[idx + 1].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx + 1]);
            if (idx - 1 >= 0 && idx - 1 < audiences.Count && audiences[idx - 1].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx - 1]);
            if (idx + tx.seat_ColNum >= 0 && idx + tx.seat_ColNum < audiences.Count && audiences[idx + tx.seat_ColNum].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx + tx.seat_ColNum]);
            if (idx - tx.seat_ColNum >= 0 && idx - tx.seat_ColNum < audiences.Count && audiences[idx - tx.seat_ColNum].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx - tx.seat_ColNum]);

            if (idx + 1 + tx.seat_ColNum >= 0 && idx + 1 + tx.seat_ColNum < audiences.Count && audiences[idx + 1 + tx.seat_ColNum].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx + 1 + tx.seat_ColNum]);
            if (idx - 1 + tx.seat_ColNum >= 0 && idx - 1 + tx.seat_ColNum < audiences.Count && audiences[idx - 1 + tx.seat_ColNum].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx - 1 + tx.seat_ColNum]);
            if (idx + 1 - tx.seat_ColNum >= 0 && idx + 1 - tx.seat_ColNum < audiences.Count && audiences[idx + 1 - tx.seat_ColNum].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx + 1 - tx.seat_ColNum]);
            if (idx - 1 - tx.seat_ColNum >= 0 && idx - 1 - tx.seat_ColNum < audiences.Count && audiences[idx - 1 - tx.seat_ColNum].socialGroup == null && URandom.value > 0.25f)
                neighbors.Add(audiences[idx - 1 - tx.seat_ColNum]);

            if (neighbors.Count > 0)
            {
                neighbors.Add(audiences[idx]);
                var groupObj = new GameObject("Social Group", typeof(SocialGroup));
                groupObj.transform.parent = transform;
                groupObj.GetComponent<SocialGroup>().members = neighbors;
                socialGroups.Add(groupObj.GetComponent<SocialGroup>());
                foreach (Audience person in neighbors)
                    person.socialGroup = groupObj.GetComponent<SocialGroup>();
            }

        }
    }

    private void Awake()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        if (_behaviorTree == null)
            CreateBehaviorTree();

        CreateCrowd();

    }

    private void Start()
    {
        if (_behaviorTree != null)
        {
            StartCoroutine(Simulate_CR());
            StartCoroutine(UpdateSocialGroup_CR());
            StartCoroutine(UpdateGazeEffect_CR());
            StartCoroutine(UpdateVoice_CR());
        }
    }

    private IEnumerator UpdateGazeEffect_CR()
    {
        while (true)
        {
            gazeCollision.UpdateGazeContact();
            yield return new WaitForSeconds(stepIntervalInput);
        }
    }

    private IEnumerator UpdateSocialGroup_CR()
    {
        while (true)
        {
            socialGroups[URandom.Range(0, socialGroups.Count)].UpdateChatStatus();
            yield return new WaitForSeconds(genChatPeriod);
        }
    }

    private IEnumerator UpdateVoice_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(voiceUpdatePeriod);
            recordWrapper.UpdateFluencyScore();
        }
    }

    private IEnumerator Simulate_CR()
    {
        //init round
        for (int i = 0; i < audienceNum; ++i)
            _behaviorTree.NextTick(audiences[i]);

        //update rounds
        while (true)
        {
            //(audiences);
            for (int i = 0; i < audienceNum; ++i)
            {
                _behaviorTree.NextTick(audiences[i]);
                yield return new WaitForSeconds(stepIntervalExt);
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = URandom.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
