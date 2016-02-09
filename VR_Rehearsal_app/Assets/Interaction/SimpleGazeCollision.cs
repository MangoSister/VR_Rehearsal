/* SimpleGazeCollision.cs
 * Yang Zhou, last modified on Feb 08, 2016
 * SimpleGazeCollision provides a simple but fast way to detect presenter's gazing behavior
 * and evaluate its influence on audience.
 * 1. Project virtual audiences' head position on a collision plane
 * 2. Perform a raycast from presenter's head to the collision plane and calculate contact point
 * 3. Presenter's gazing influence follows a 2d gaussian distribution centered at contact
 * 4. Each audience then query the influence
 * now collision plane is manually placed, may consider auto fit later (least squared error?)
 * may also considered more sophisticated collision surface shape (quadratic, hyperpola?)
 * Dependencies: need PresenterHead transform in unity scene
 */

using UnityEngine;

public class SimpleGazeCollision : MonoBehaviour
{
    //utility plane property
    private Plane collisionPlane
    { get { return new Plane(collisionPlaneNormal, collisionPlaneCenter); } }

    //use transform.position and transform.up to define the collision plane
    public Vector3 collisionPlaneCenter { get { return transform.position; } }
    public Vector3 collisionPlaneNormal { get { return transform.up; } }

    //transform of presenter's head (dragged it in Unity Editor)
    public Transform presenterHead;

    //define gaussian distribution for gazing power
    public float peak = 1.0f; //amplitude of distribution
    public float range = 1.0f; //variance of distribution

    //contact point is stored here
    public Vector3? _lastContactPoint { get; private set; }

#if UNITY_EDITOR
    //for debug, visualization range of collision plane
    public float debugPlaneRange = 1.0f;
#endif

    //Use this to update gaze contact point
    public void UpdateGazeContact()
    {
        Ray ray = new Ray(presenterHead.position, presenterHead.forward);
        float enter;
        if (collisionPlane.Raycast(ray, out enter))
            _lastContactPoint = ray.GetPoint(enter);
        else _lastContactPoint = null;
    }

    //Use this to evaluate gaze influence for each audience
    //audiencePos: HEAD position of audience
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
#endif


}
