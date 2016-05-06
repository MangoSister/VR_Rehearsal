/* SimpleGazeCollision.cs
 * Yang Zhou, last modified on Match 2nd, 2016
 * SimpleGazeCollision provides a simple but fast way to detect presenter's gazing behavior
 * and evaluate its influence on audience.
 * 1. Project virtual audiences' head position on a collision plane
 * 2. Perform a raycast from presenter's head to the collision plane and calculate contact point
 * 3. Presenter's gazing influence follows a 2d gaussian distribution centered at contact
 * 4. Each audience then query the influence
 * now collision plane is manually placed, may consider auto fit later (least squared error?)
 * may also considered more sophisticated collision surface shape (quadratic, hyperpola?)
 * Dependencies: need PresenterHead transform in unity scene

 * Now separate two axes to get better estimation
 */

using UnityEngine;
using System.Collections.Generic;

public class SimpleGazeCollision : MonoBehaviour
{
    //query external looking direction information? (eye tracking)
    public bool eyetracking = false;
    public EyeCoordHandler UpdateEyeCoord;

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
    public float rangeForward = 1.0f; //variance of distribution, forward component
    public float rangeRight = 5.0f; //variance of distribution, right component
    //contact point is stored here
    public Vector3? _lastContactPoint { get; private set; }
    public Vector3? _lastAxisForward { get; private set; }
    public Vector3? _lastAxisRight { get; private set; }
#if UNITY_EDITOR
    //for debug, visualization range of collision plane
    public float debugPlaneRange = 1.0f;
#endif

    //Use this to update gaze contact point
    public void UpdateGazeContact()
    {
        Ray ray;
        if (!eyetracking)
            ray = new Ray(presenterHead.position, presenterHead.forward);
        else ray = new Ray(presenterHead.position, UpdateEyeCoord().normalized);
        float enter;
        if (collisionPlane.Raycast(ray, out enter))
        {
            _lastContactPoint = ray.GetPoint(enter);
            _lastAxisForward = Vector3.ProjectOnPlane(ray.direction, collisionPlaneNormal).normalized;
            _lastAxisRight = Vector3.Cross(_lastAxisForward.Value, collisionPlaneNormal);
        }
        else
        {
            _lastContactPoint = null;
            _lastAxisForward = null;
            _lastAxisRight = null;
        }
    }

    //Use this to evaluate gaze influence for each audience
    //audiencePos: HEAD position of audience
    public float EvaluateGazePower(Vector3 audiencePos)
    {
        if (presenterHead == null || !_lastContactPoint.HasValue)
            return 0f;
        var plane = collisionPlane;

        Vector3 projDisp = (audiencePos - _lastContactPoint.Value) - plane.GetDistanceToPoint(audiencePos) * plane.normal;
        float df = Vector3.Dot(projDisp, _lastAxisForward.Value);
        float dr = Vector3.Dot(projDisp, _lastAxisRight.Value);
        float power = peak * Mathf.Exp(-0.5f * (df * df / rangeForward + dr * dr / rangeRight));
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
        Vector3 center, axisF, axisR;
        if (_lastContactPoint.HasValue)
        {
            center = _lastContactPoint.Value;
            axisF = _lastAxisForward.Value;
            axisR = _lastAxisRight.Value;
        }
        else
        {
            center = collisionPlaneCenter;
            axisF = Vector3.ProjectOnPlane( Vector3.forward, collisionPlaneNormal).normalized;
            axisR = Vector3.Cross(axisF, collisionPlaneNormal);
        }

        for (float val = 0.1f; val <= 0.9f; val += 0.2f)
        {
            float radius = Mathf.Sqrt
                (-2f * Mathf.Log(1f / peak * val));
            
            if (_lastContactPoint.HasValue)
                UnityEditor.Handles.color = Color.red * new Color(val, val, val, 1f);
            else
                UnityEditor.Handles.color = Color.yellow * new Color(val, val, val, 1f);

            //draw ellipse
            List<Vector3> samples = new List<Vector3>();
            
            for (float theta = 0f; theta < 360f; theta += 10f)
            {
                samples.Add
                (Mathf.Cos((Mathf.Deg2Rad * theta)) * axisF * Mathf.Sqrt(rangeForward) * radius +
                Mathf.Sin(Mathf.Deg2Rad * theta) * axisR * Mathf.Sqrt(rangeRight) * radius + center);
            }
            UnityEditor.Handles.DrawAAPolyLine(samples.ToArray());
        }
        UnityEditor.Handles.color = oldColor;

    }
#endif


}
