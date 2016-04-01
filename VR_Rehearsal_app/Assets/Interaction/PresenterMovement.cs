using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void PresenterMove_Handler(DestType dest);
public enum DestType { PODIUM, DOOR_IN, DOOR_OUT };

[RequireComponent(typeof(NavMeshAgent))]
public class PresenterMovement : MonoBehaviour
{
    public event PresenterMove_Handler OnPreMove;
    public event PresenterMove_Handler OnPostMove;

    private Dictionary<DestType, Transform> _destDict;
    private DestType _currDest;
    private bool _isMove;

    private NavMeshAgent _agent { get { return GetComponent<NavMeshAgent>(); } }
    private bool _isArrived
    {
        get
        {
            if (!_agent.pathPending &&
                _agent.remainingDistance <= _agent.stoppingDistance &&
                (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f))
            {
                return true;
            }
            else return false;
        }

    }
    
    private void Start()
    {
        _agent.autoTraverseOffMeshLink = false;
        _destDict = new Dictionary<DestType, Transform>
        {
            { DestType.PODIUM, SceneController.currRoom.presentDest},
            { DestType.DOOR_IN, SceneController.currRoom.roomDoorIn},
            { DestType.DOOR_OUT, SceneController.currRoom.roomDoorOut},
        };

        StartCoroutine(ProcessOffMeshLink_CR());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            MoveTo(DestType.PODIUM);
        if (Input.GetKeyDown(KeyCode.W))
            MoveTo(DestType.DOOR_OUT);
    }

    private IEnumerator ProcessOffMeshLink_CR()
    {
        while (true)
        {
            if (_agent.isOnOffMeshLink)
            {
                GameObject proxyGo = new GameObject("AreaTransitionProxy", typeof(AreaTransitionProxy));
                var proxy = proxyGo.GetComponent<AreaTransitionProxy>();
                proxy.agent = _agent;       
                proxy.offMeshLinkData = _agent.currentOffMeshLinkData;
                proxy.transitionData = proxy.offMeshLinkData.offMeshLink.gameObject.GetComponent<AreaTransitionData>();
                yield return StartCoroutine(proxy.Transition_CR());
                _agent.CompleteOffMeshLink();
            }
            yield return null;
        }
    }

    public void MoveTo(DestType dest)
    {
        if (_isMove)
            return;

        if (OnPreMove != null)
            OnPreMove(dest);

        _isMove = true;
        _currDest = dest;
        _agent.SetDestination(_destDict[dest].position);
        StartCoroutine(PostMove_CR());
    }

    private IEnumerator PostMove_CR()
    {
        while (!_isArrived)
            yield return null;

        switch (_currDest)
        {
            case DestType.PODIUM:
                {
                    float time = Quaternion.Angle(transform.rotation, SceneController.currRoom.presentDest.rotation) / (_agent.angularSpeed * 0.1f);
                    float currTime = 0f;
                    Quaternion startRot = transform.rotation;
                    while (transform.rotation != SceneController.currRoom.presentDest.rotation)
                    {
                        currTime += Time.deltaTime;
                        transform.rotation = Quaternion.Slerp(startRot, SceneController.currRoom.presentDest.rotation,
                        Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(currTime / time)));
                        yield return null;
                    }
                    break;
                }
            case DestType.DOOR_IN: break;
            case DestType.DOOR_OUT: break;
            default: break;
        }

        if (OnPostMove != null)
            OnPostMove(_currDest);

        _isMove = false;

    }
    
}