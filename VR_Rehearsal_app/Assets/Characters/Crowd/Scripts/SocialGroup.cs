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
}
