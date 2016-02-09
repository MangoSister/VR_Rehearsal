using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SimpleGazeCollision : MonoBehaviour
{
    public Plane collisionPlane
    { get { return new Plane(collisionPlaneNormal, collisionPlaneCenter); } }
    public Vector3 collisionPlaneCenter { get { return transform.position; } }
    public Vector3 collisionPlaneNormal { get { return transform.up; } }

    public Transform presenterHead;

    public float peak = 1.0f;
    public float range = 1.0f;

    public Vector3? _lastContactPoint { get; private set; }

#if UNITY_EDITOR
    public float debugPlaneRange = 1.0f;
#endif

    public void UpdateGazeContact()
    {
        Ray ray = new Ray(presenterHead.position, presenterHead.forward);
        float enter;
        if (collisionPlane.Raycast(ray, out enter))
            _lastContactPoint = ray.GetPoint(enter);
        else _lastContactPoint = null;
    }

    public float EvaluateGazePower(Vector3 audiencePos)
    {
        if (presenterHead == null || !_lastContactPoint.HasValue)
            return 0f;
        var plane = collisionPlane;
        float power = peak * Mathf.Exp
                    (-(0.5f / range *
                    ((audiencePos - _lastContactPoint) - plane.GetDistanceToPoint(audiencePos) * plane.normal).Value.sqrMagnitude));
        return power;

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawPlane(collisionPlaneCenter, collisionPlaneNormal);
    }
#endif

    private void DrawPlane(Vector3 pos, Vector3 normal)
    {
        Vector3 v3;

        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude;

        v3 *= debugPlaneRange;

        var corner0 = pos + v3;
        var corner2 = pos - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = pos + v3;
        var corner3 = pos - v3;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(corner0, corner2);
        Gizmos.DrawLine(corner1, corner3);
        Gizmos.DrawLine(corner0, corner1);
        Gizmos.DrawLine(corner1, corner2);
        Gizmos.DrawLine(corner2, corner3);
        Gizmos.DrawLine(corner3, corner0);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos, normal);

        Color oldColor = UnityEditor.Handles.color;
        for (float val = 0.1f; val <= 0.9f; val += 0.2f)
        {
            float r = Mathf.Sqrt
                (-2f * range * range * Mathf.Log(1f / peak * val));

            if (_lastContactPoint.HasValue)
            {
                UnityEditor.Handles.color = Color.red * new Color(val, val, val, 1f);
                UnityEditor.Handles.DrawWireDisc(_lastContactPoint.Value, collisionPlaneNormal, r);
            }
            else
            {
                UnityEditor.Handles.color = Color.yellow * new Color(val, val, val, 1f);
                UnityEditor.Handles.DrawWireDisc(collisionPlaneCenter, collisionPlaneNormal, r);
            }
        }
        UnityEditor.Handles.color = oldColor;
    }
}
