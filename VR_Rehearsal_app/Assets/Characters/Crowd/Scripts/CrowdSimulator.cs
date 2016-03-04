using UnityEngine;
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
    public float stepInterval;
    public float stepExternalInterval;

    public SimModule simModule = 
        SimModule.Gaze | SimModule.VoiceVolume | 
        SimModule.FillerWords | SimModule.SeatDistribution | 
        SimModule.SocialGroup | SimModule.Global;

    public bool deterministic;

    public float globalAttentionMean { get; set; }
    public float globalAttentionStDev { get; set; }
    public AnimationCurve seatPosAttentionFactor;
    public float seatPosAttentionUpper
    {
        get
        {
            return seatPosAttentionFactor.keys[0].value;
        }
        set
        {
            var keys = seatPosAttentionFactor.keys;
            keys[0].value = value;
            seatPosAttentionFactor = new AnimationCurve(keys);

        }
    }
    public float seatPosAttentionLower
    {
        get
        {
            return seatPosAttentionFactor.keys[1].value;
        }
        set
        {
            var keys = seatPosAttentionFactor.keys;
            keys[1].value = value;
            seatPosAttentionFactor = new AnimationCurve(keys);
        }
    }

    public SimpleGazeCollision gazeCollision;

    [Range(0f,1f)]
    public float gazeCumulativeIntensity;

    private List<Audience> audiences;
    public int audienceNum { get { return audiences.Count; } }
    private List<SocialGroup> socialGroups;

    //private BehaviorTree<Audience> _audienceBt = null;
    private BehaviorTree<Audience> _behaviorTree;

    private void CreateDummyTree()
    {
        _behaviorTree = new BehaviorTree<Audience>(
            new SequenceNode<Audience>(
                new AudienceSimStepNode(),
                new AudienceStateSelectorNode(1,
                    new List<BaseNode<Audience>>
                    {
                        //new AudienceStateNode(Audience.States.Focused),
                        //new AudienceStateNode(Audience.States.Bored),
                        //new AudienceStateNode(Audience.States.Chatting)
                        new AudienceFocusStateNode(),
                        new AudienceBoredStateNode(),
                        new AudienceChatStateNode()
                    })
                ));
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
            ad.followingTransform = RoomCenter.currRoom.presenterHead;
            ad.GetComponent<AudienceAnimHandler>().repeatPeriodBound = new Vector2(10f, 20f);

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
                SocialGroup group = new SocialGroup(neighbors);
                socialGroups.Add(group);
                foreach (Audience person in neighbors)
                    person.socialGroup = group;
            }

        }
    }

    private void Awake()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        globalAttentionMean = 0.6f;
        globalAttentionStDev = 0.05f;

        if (_behaviorTree == null)
            CreateDummyTree();

        CreateCrowd();

    }

    private void Start()
    {
        if (_behaviorTree != null)
        {
            StartCoroutine(Simulate_CR());
            StartCoroutine(ExternalFactor_CR());
        }
    }

    private IEnumerator Simulate_CR()
    {
        float stepPerAudience = stepInterval / audienceNum;
        while (true)
        {
            foreach (SocialGroup group in socialGroups)
                group.isComputed = false;

            Shuffle(audiences);
            for (int i = 0; i < audiences.Count; ++i)
            {
                _behaviorTree.NextTick(audiences[i]);
                yield return new WaitForSeconds(stepPerAudience);
            }
        }
    }

    private IEnumerator ExternalFactor_CR()
    {
        while (true)
        {
            gazeCollision.UpdateGazeContact();
            yield return new WaitForSeconds(stepExternalInterval);
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
