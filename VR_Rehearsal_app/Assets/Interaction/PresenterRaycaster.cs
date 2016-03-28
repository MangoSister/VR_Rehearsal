using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PresenterRaycaster : MonoBehaviour
{
    public float upOffset;
    public float range;
    public LayerMask raycastLayerMask;

    private RaycastReceiver _prevTarget;
    public Transform headTransform;

    private void Start()
    {
        if (headTransform == null)
            headTransform = GameObject.Find("Head").transform;
    }

    private void FixedUpdate()
    {
        Ray ray = new Ray(headTransform.position + headTransform.up * upOffset,
                            headTransform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range, raycastLayerMask) &&
            hit.collider.GetComponent<RaycastReceiver>() != null &&
            !(hit.collider.GetComponent<MeshRenderer>() != null && !hit.collider.GetComponent<MeshRenderer>().isVisible)
            )
        {
            RaycastReceiver _currTarget = hit.collider.GetComponent<RaycastReceiver>();
            if (_prevTarget != null && _prevTarget != _currTarget)
            {
                _prevTarget.OnEyeExit();
                _currTarget.OnEyeEnter();
                _prevTarget = _currTarget;
            }
            else if (_prevTarget == null)
            {
                _currTarget.OnEyeEnter();
                _prevTarget = _currTarget;
            }
        }
        else if (_prevTarget != null)
        {
            _prevTarget.OnEyeExit();
            _prevTarget = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!headTransform)
            return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine
            (headTransform.position + headTransform.up * upOffset, 
            headTransform.position + headTransform.up * upOffset + headTransform.forward * range);
        Gizmos.color = Color.white;
        Gizmos.DrawLine
            (headTransform.position + headTransform.up * upOffset,
            headTransform.position + headTransform.up * upOffset + headTransform.forward * range * 5);
    }
#endif
}
