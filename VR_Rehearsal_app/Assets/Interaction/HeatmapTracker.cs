/* HeatmapTracker.cs
 * Yang Zhou, last modified on Mar 3, 2016
 * HeatmapTracker stores the presenter's gaze data
 * Dependencies: need RoomCenter in VR scene, Cardboard post render object for editor ONLY screen overlay
 * May implement interpolation in the future

 * Now be able to capture a screenshot (with edge enhancement) with a separate camera
 * The actual heatmap generation relies on HeatmapGenerator.cs
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

//the structure to store the information per update
public struct GazeSnapshot
{
    public float timeStamp;
    public Vector3 headToBodyDir;
}

public class HeatmapTracker : MonoBehaviour
{
    //updating interval. Accumulate once per updateInterval
    public float updateInteval = 1f;

    //heatmaptracker works similar to a camera, i.e., projects the presenter's view direction on a XY plane at z = 1
    //vertical "field of view" (in degrees) of heatmap, horizontal FOV can be computed with it and (screen) aspect
    //view direction outside FOV would contribute to "out of bound" component
    public float _verticalFOVDeg = 60f;

    public float verticalFOVDeg
    {
        get { return _verticalFOVDeg; }
        set
        {
            _verticalFOVDeg = value;
            _aspect = Mathf.Tan(Mathf.Deg2Rad * 0.5f * _horizontalFOVDeg) /
              Mathf.Tan(Mathf.Deg2Rad * 0.5f * _verticalFOVDeg);
        }
    }

    public float _horizontalFOVDeg = 120f;

    public float horizontalFOVDeg
    {
        get { return _horizontalFOVDeg;}
        set
        {
            _horizontalFOVDeg = value;
            _aspect = Mathf.Tan(Mathf.Deg2Rad * 0.5f * _horizontalFOVDeg) /
              Mathf.Tan(Mathf.Deg2Rad * 0.5f * _verticalFOVDeg);
        }
    }

    private float? _aspect;

    //Screen aspect ratio
    public float aspect
    {
        get
        {
            if (!_aspect.HasValue)
            {
                _aspect = Mathf.Tan(Mathf.Deg2Rad * 0.5f * _horizontalFOVDeg) /
              Mathf.Tan(Mathf.Deg2Rad * 0.5f * _verticalFOVDeg);
            }
            return _aspect.Value;
        }
        set
        { _aspect = value; }
    }

    //Gaze data: a series of GazeSnapshot (temporal)
    private List<GazeSnapshot> _gazeData = new List<GazeSnapshot>();

    //the time scale of replay would be (real world time scale) * replaySpeed
    public float replaySpeed = 5f;

    private Coroutine _currUpdateCR = null;

#if UNITY_EDITOR
    private bool _replayOverlay = false;
#endif

    public Texture2D scn;
    public Camera scnCam;
    public int scnWidthRes = 1024;
    private void Start()
    {
        _gazeData.Clear();
        StartCoroutine(Capture_CR());
    }

    //In VR scene, call me to start track
    //return: whether the tracking is triggered successfully
    public bool StartTrack()
    {
        if (_currUpdateCR == null && SceneController.currRoom != null)
        {
            _currUpdateCR = StartCoroutine(Update_CR());
            return true;
        }
        else return false;
    }

    //In VR scene, call me to stop track
    //return: whether the tracking is stopped successfully
    public bool StopTrack()
    {
        if (_currUpdateCR != null)
        {
            StopCoroutine(_currUpdateCR);
            _currUpdateCR = null;
            return true;
        }
        else return false;
    }

    //Clear track data (_gazeData)
    public void ClearTrackData()
    {
        _gazeData.Clear();
    }

    private IEnumerator Update_CR()
    {
        var head = SceneController.currRoom.presenterHead;
        var body = SceneController.currRoom.presenter.transform;
        while (true)
        {
            float z = Vector3.Dot(head.forward, body.forward);
            float x = Vector3.Dot(head.forward, body.right);
            float y = Vector3.Dot(head.forward, body.up);
            GazeSnapshot snapshot = new GazeSnapshot();
            snapshot.headToBodyDir = new Vector3(x, y, z);
            snapshot.timeStamp = Time.time;
            _gazeData.Add(snapshot);

            yield return new WaitForSeconds(updateInteval);
        }
    }

    private void OnEnable()
    {
        StartTrack();
    }

    private void OnDisable()
    {
        StopTrack();
    }

    public List<GazeSnapshot> output
    {
        get
        {
            List<GazeSnapshot> output = new List<GazeSnapshot>();
            foreach (GazeSnapshot snapshot in _gazeData)
                output.Add(snapshot);
            return output;
        }
    }

    public void CaptureScreenshot()
    {
        scnCam.gameObject.SetActive(true);
        scnCam.gameObject.transform.position = SceneController.currRoom.presenterHead.position;
        scnCam.fieldOfView = verticalFOVDeg;
        scnCam.aspect = aspect;

        RenderTexture rt = new RenderTexture
            (scnWidthRes, (int)((float)scnWidthRes / aspect), 24, RenderTextureFormat.Default);
        scnCam.targetTexture = rt;
        scnCam.Render();
        RenderTexture oldActive = RenderTexture.active;
        RenderTexture.active = rt;

        scn = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        scn.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        scn.Apply();

        RenderTexture.active = oldActive;

        scnCam.gameObject.SetActive(false);
    }

    private IEnumerator Capture_CR()
    {
        scnCam.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        CaptureScreenshot();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Matrix4x4 oldMat = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(SceneController.currRoom.presenterHead.position, 
                                        SceneController.currRoom.presenter.transform.rotation, 
                                        Vector3.one);
        Gizmos.DrawFrustum
        (SceneController.currRoom.presenterHead.position, verticalFOVDeg, 5f, 1f, aspect);
        Gizmos.matrix = oldMat;
        Gizmos.color = Color.white;

    }


    //    //screen overlay routine in editor ONLY
    //    //need to be plugged to Cardboard post render object
    //    public void RenderOverlay()
    //    {
    //        if (!_replayOverlay)
    //            return;

    //        GL.PushMatrix();
    //        GL.LoadOrtho();

    //        _overLayMat.SetPass(0);

    //        GL.Begin(GL.QUADS);

    //        GL.TexCoord(new Vector3(0, 0, 0));
    //        GL.Vertex3(0, 0, 0);

    //        GL.TexCoord(new Vector3(1, 0, 0));
    //        GL.Vertex3(1, 0, 0);

    //        GL.TexCoord(new Vector3(1, 1, 0));
    //        GL.Vertex3(1, 1, 0);

    //        GL.TexCoord(new Vector3(0, 1, 0));
    //        GL.Vertex3(0, 1, 0);

    //        GL.End();

    //        GL.PopMatrix();
    //    }
    #endif
}
