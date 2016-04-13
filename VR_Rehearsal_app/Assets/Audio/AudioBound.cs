using UnityEngine;
using System.Collections;

public class AudioBound : MonoBehaviour
{
    public Bounds bound;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(bound.center + transform.position, bound.size);
        Gizmos.color = Color.white;
    }
#endif
}
