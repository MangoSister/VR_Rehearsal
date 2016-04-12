using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshRenderer))]
public class CrossHair : MonoBehaviour
{
  /// Number of segments making the reticle circle.
  public int reticleSegments = 20;

  /// Growth speed multiplier for the reticle/
  public float reticleGrowthSpeed = 8.0f;

  // Private members
  private Material materialComp;
  private GameObject targetObj;

  // Current inner angle of the reticle (in degrees).
  private float reticleInnerAngle = 0.0f;
  // Current outer angle of the reticle (in degrees).
  private float reticleOuterAngle = 0.5f;
  // Current distance of the reticle (in meters).
  private float reticleDistanceInMeters = 10.0f;

  // Minimum inner angle of the reticle (in degrees).
  private const float kReticleMinInnerAngle = 0.0f;
  // Minimum outer angle of the reticle (in degrees).
  private const float kReticleMinOuterAngle = 0.5f;
  // Angle at which to expand the reticle when intersecting with an object
  // (in degrees).
  private const float kReticleGrowthAngle = 1.5f;

  // Minimum distance of the reticle (in meters).
  private const float kReticleDistanceMin = 0.75f;
  // Maximum distance of the reticle (in meters).
  private const float kReticleDistanceMax = 10.0f;

  // Current inner and outer diameters of the reticle,
  // before distance multiplication.
  private float reticleInnerDiameter = 0.0f;
  private float reticleOuterDiameter = 0.0f;

  void Start () {
    CreateReticleVertices();

    materialComp = gameObject.GetComponent<MeshRenderer>().material;
  }

  /// Called when the user is looking on a valid GameObject. This can be a 3D
  /// or UI element.
  ///
  /// The camera is the event camera, the target is the object
  /// the user is looking at, and the intersectionPosition is the intersection
  /// point of the ray sent from the camera on the object.
  public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition) {
    SetGazeTarget(intersectionPosition);
  }

  private void CreateReticleVertices() {
    Mesh mesh = new Mesh();
    gameObject.AddComponent<MeshFilter>();
    GetComponent<MeshFilter>().mesh = mesh;

    int segments_count = reticleSegments;
    int vertex_count = (segments_count+1)*2;

    #region Vertices

    Vector3[] vertices = new Vector3[vertex_count];

    const float kTwoPi = Mathf.PI * 2.0f;
    int vi = 0;
    for (int si = 0; si <= segments_count; ++si) {
      // Add two vertices for every circle segment: one at the beginning of the
      // prism, and one at the end of the prism.
      float angle = (float)si / (float)(segments_count) * kTwoPi;

      float x = Mathf.Sin(angle);
      float y = Mathf.Cos(angle);

      vertices[vi++] = new Vector3(x, y, 0.0f); // Outer vertex.
      vertices[vi++] = new Vector3(x, y, 1.0f); // Inner vertex.
    }
    #endregion

    #region Triangles
    int indices_count = (segments_count+1)*3*2;
    int[] indices = new int[indices_count];

    int vert = 0;
    int idx = 0;
    for (int si = 0; si < segments_count; ++si) {
      indices[idx++] = vert+1;
      indices[idx++] = vert;
      indices[idx++] = vert+2;

      indices[idx++] = vert+1;
      indices[idx++] = vert+2;
      indices[idx++] = vert+3;

      vert += 2;
    }
    #endregion

    mesh.vertices = vertices;
    mesh.triangles = indices;
    mesh.RecalculateBounds();
    mesh.Optimize();
  }

  private void UpdateDiameters() {
    reticleDistanceInMeters =
      Mathf.Clamp(reticleDistanceInMeters, kReticleDistanceMin, kReticleDistanceMax);

    if (reticleInnerAngle < kReticleMinInnerAngle) {
      reticleInnerAngle = kReticleMinInnerAngle;
    }

    if (reticleOuterAngle < kReticleMinOuterAngle) {
      reticleOuterAngle = kReticleMinOuterAngle;
    }

    float inner_half_angle_radians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
    float outer_half_angle_radians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;

    float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
    float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

    reticleInnerDiameter =
        Mathf.Lerp(reticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
    reticleOuterDiameter =
        Mathf.Lerp(reticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);

    materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
    materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
    materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
  }

  private void SetGazeTarget(Vector3 target) {
    Vector3 targetLocalPosition = transform.parent.InverseTransformPoint(target);

    reticleDistanceInMeters =
        Mathf.Clamp(targetLocalPosition.z, kReticleDistanceMin, kReticleDistanceMax);
    reticleInnerAngle = kReticleMinInnerAngle + kReticleGrowthAngle;
    reticleOuterAngle = kReticleMinOuterAngle + kReticleGrowthAngle;
  }
}