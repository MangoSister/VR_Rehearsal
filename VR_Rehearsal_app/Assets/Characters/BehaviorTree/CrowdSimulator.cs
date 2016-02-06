using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MangoBehaviorTree;

public class CrowdSimulator : MonoBehaviour
{
    public bool dummy = true;
    public float stepTime;
    private List<Audience> audiences;

    private BehaviorTree<Audience> _audienceBt = null;

    private BehaviorTree<Audience> _dummyAudienceBt;

    private void CreateDummyTree()
    {
        _dummyAudienceBt = new BehaviorTree<Audience>(
            new SequenceNode<Audience>(
                new WaitNode<Audience>(stepTime),
                new AudienceStateSelectorNode(1,
                    new List<BaseNode<Audience>>
                    {
                        new AudienceStateNode(Audience.States.Focused),
                        new AudienceStateNode(Audience.States.Bored),
                        new AudienceStateNode(Audience.States.Chatting)
                    })
                ));
    }

    private void Awake()
    {
        if (_dummyAudienceBt == null)
            CreateDummyTree();

        audiences = new List<Audience>(GetComponentsInChildren<Audience>());

    }

    private void Update()
    {
        if (dummy && _dummyAudienceBt != null)
        {
            foreach(Audience audience in audiences)
                _dummyAudienceBt.NextTick(audience);
        }        
    }

}
