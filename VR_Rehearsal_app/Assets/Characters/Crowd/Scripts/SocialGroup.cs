/* HeatmapTracker.cs
 * Yang Zhou, last modified on Feb 20, 2016
 * HeatmapTracker stores the presenter's gaze data, generate heatmap based on the data,
 * and performs gaze trajectory replay.
 * Dependencies: need RoomCenter in VR scene, Cardboard post render object for editor ONLY screen overlay
 * May implement interpolation in the future
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SocialGroup
{
    //public int groupId;
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

    public bool isComputed = false;
    public bool requestChat = false;

    public SocialGroup(List<Audience> members)
    {
        this.members = members;
       // groupId = NextGlobalId;
    }

    //private static int _globalId = 0;
    //public static int NextGlobalId
    //{
    //    get
    //    {
    //        if (_globalId == int.MaxValue)
    //            throw new Exception("cannot assign id anymore");
    //        return _globalId++;
    //    }
    //}
}
