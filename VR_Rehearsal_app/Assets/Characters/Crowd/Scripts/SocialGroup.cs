using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SocialGroup : MonoBehaviour
{
    public List<Audience> members;
    public Vector3 centerPos
    {
        get
        {
            Vector3 output = Vector3.zero;
            if (members.Count > 0)
            {
                foreach (Audience member in members)
                    output += member.transform.position;
                output /= (float)members.Count;
            }
            return output;
        }
    }

    public bool shouldChat = false;

    public void UpdateChatStatus()
    {
        double avgAttention = 0f;
        foreach (Audience member in members)
        {
            if (member.attention > CrowdSimulator.currSim.noChatThreshold)
            {
                shouldChat = false;
                return;
            }
            avgAttention += member.attention;
        }

        avgAttention /= members.Count;
        if (avgAttention < CrowdSimulator.currSim.avgChatThreshold)
        {
            shouldChat = true;
            StopAllCoroutines();
            StartCoroutine(Chat_CR());
        }
        else shouldChat = false;
    }

    private IEnumerator Chat_CR()
    {
        yield return new WaitForSeconds(CrowdSimulator.currSim.chatLength);
        shouldChat = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (members.Count == 0 || !shouldChat)
            return;

        Color oldColor = Gizmos.color;
        Gizmos.color = Color.red;
        foreach (Audience ad in members)
            Gizmos.DrawCube(ad.transform.position, Vector3.one * 0.5f);
        Gizmos.color = oldColor;
    }
#endif

}
