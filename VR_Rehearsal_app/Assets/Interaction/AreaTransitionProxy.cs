using UnityEngine;
using System.Collections;
using MoveType = AreaTransitionData.TransistionMoveType;

public class AreaTransitionProxy : MonoBehaviour
{
    public AreaTransitionData transitionData;
    public OffMeshLinkData offMeshLinkData;
    public NavMeshAgent agent;

    //Ignore rotation for now...
    public IEnumerator Transition_CR()
    {
        transitionData.OnPreTransition.Invoke(true, offMeshLinkData.endPos == offMeshLinkData.offMeshLink.endTransform.position);

        yield return new WaitForSeconds(transitionData.preWaitTime);

        switch (transitionData.moveType)
        {
            case MoveType.Teleport: default:
                { yield return StartCoroutine(Teleport_CR());  break; }
            case MoveType.Walk:
                { yield return StartCoroutine(Walk_CR(transitionData.transitionSpeed)); break; }
            case MoveType.Parabola:
                { yield return StartCoroutine(Parabola_CR(transitionData.parabolaHeight, transitionData.transitionSpeed)); break; }
        }

        transitionData.OnPostTransition.Invoke(false, offMeshLinkData.endPos == offMeshLinkData.offMeshLink.endTransform.position);
        yield return new WaitForSeconds(transitionData.postWaitTime);
        Destroy(gameObject);
        yield return null;
    }

    private IEnumerator Teleport_CR()
    {
        agent.transform.position = offMeshLinkData.endPos + Vector3.up * agent.baseOffset;
        yield return null;
    }

    private IEnumerator Walk_CR(float speed)
    {
        Vector3 endPos = offMeshLinkData.endPos + Vector3.up * agent.baseOffset;
        while (agent.transform.position != endPos)
        {
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos,
                speed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator Parabola_CR(float height, float speed)
    {
        Vector3 startPos = agent.transform.position;
        Vector3 endPos = offMeshLinkData.endPos + Vector3.up * agent.baseOffset;
        float dist = Vector3.Distance(startPos, endPos);
        float normalizedTime = 0.0f;
        while (normalizedTime < 1.0f)
        {
            float yOffset = height * 4.0f * (normalizedTime - normalizedTime * normalizedTime);
            agent.transform.position = Vector3.Lerp(startPos, endPos, normalizedTime) + yOffset * Vector3.up;
            normalizedTime += Time.deltaTime / dist * speed;
            yield return null;
        }
    }
}
