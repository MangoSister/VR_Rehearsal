using UnityEngine;
using System.Collections;

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

    public AnimationClip[] basicFocusedClips;
    public AnimationClip[] basicBoredClips;
    public AnimationClip[] basicLeftChattingClips;
    public AnimationClip[] basicRightChattingClips;

    public AnimationClip[] followFocusedClips;
    public AnimationClip[] followBoredClips;
    public AnimationClip[] followLeftChattingClips;
    public AnimationClip[] followRightChattingClips;

}
