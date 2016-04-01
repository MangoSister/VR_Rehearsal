using UnityEngine;
using System.Collections;

public class AudienceAnimClipHolder : MonoBehaviour
{
    private static AudienceAnimClipHolder _curr;
    public static AudienceAnimClipHolder curr
    {
        get
        {
            if (_curr == null)
                _curr = FindObjectOfType<AudienceAnimClipHolder>();
            return _curr;
        }
    }

    public RuntimeAnimatorController basicController;
    public RuntimeAnimatorController followController;
    public RuntimeAnimatorController fullController;

    public AnimationClip[] focusedClips;
    public AnimationClip[] boredClips;
    public AnimationClip[] chattingClips;

}
