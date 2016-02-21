/* HeatmapTracker.cs
 * Yang Zhou, last modified on Feb 20, 2016
 * HeatmapTracker stores the presenter's gaze data, generate heatmap based on the data,
 * and performs gaze trajectory replay.
 * Dependencies: need RoomCenter in VR scene, Cardboard post render object for editor ONLY screen overlay
 * May implement interpolation in the future
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HeatmapTracker : MonoBehaviour
{
    //The color pattern of heatmap
    public Gradient heatmapGradient;

    //The curve used to map staring time (relative to query interval) to gradient color
    //instead of simple linear lerp, a curve provides more flexibility and more reasonable mapping
    public AnimationCurve heatmapColorCurve;

    //heatmaptracker works similar to a camera, i.e., projects the presenter's view direction on a XY plane at z = 1
    //vertical "field of view" (in degrees) of heatmap, horizontal FOV can be computed with it and (screen) aspect
    //view direction outside FOV would contribute to "out of bound" component
    public float verticalFOVDeg = 60f;

    //updating interval. Accumulate once per updateInterval
    public float updateInteval = 1f;

    //private structure to store the information per update
    private struct GazeSnapshot
    {
        public float timeStamp;
        public Vector3 headToBodyDir;
    }

    //Gaze data: a series of GazeSnapshot (temporal)
    private List<GazeSnapshot> _gazeData = new List<GazeSnapshot>();

    //The width of generated heatmap. The height of the map depends on aspect
    public int widthResolution = 256;

    //Screen aspect ratio
    private float aspect
    { get { return (float)Screen.width / (float)Screen.height; } }

    //screen overlay material (for unity editor ONLY)
#if UNITY_EDITOR
    private Material _overLayMat;
#endif

    //the time scale of replay would be (real world time scale) * replaySpeed
    public float replaySpeed = 5f;

    private Coroutine _currUpdateCR = null;
    private Coroutine _currReplayCR = null;

    //Replay finish callback
    public delegate void ReplayFinish_Handler();
    public ReplayFinish_Handler OnReplayFinish;

#if UNITY_EDITOR
    private bool _replayOverlay = false;
#endif

    private void Start()
    {
        verticalFOVDeg = Mathf.Clamp(verticalFOVDeg, 1f, 179f);
        _gazeData.Clear();
    }

    //In VR scene, call me to start track
    //return: whether the tracking is triggered successfully
    public bool StartTrack()
    {
        if (_currUpdateCR == null && RoomCenter.currRoom != null)
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
        var head = RoomCenter.currRoom.presenterHead;
        var body = RoomCenter.currRoom.presenter.transform;
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

    //Call me to query a time period and generate the heatmap (as well as the max staring time of all directions)
    //from: query start time (0 is the time when VR scene starts)
    //to: query end time
    //heatmap: generated heatmap
    //maxTime: the max staring time of all directions
    public void GenerateMap(float from, float to, out Texture2D heatmap, out float maxTime)
    {
        float halfProjHeight = Mathf.Tan(verticalFOVDeg * 0.5f * Mathf.Deg2Rad);
        float halfProjWidth = halfProjHeight * aspect;
        float length = to - from;


        heatmap = new Texture2D(widthResolution, Mathf.RoundToInt((float)widthResolution / aspect),
                                 TextureFormat.ARGB32, false);
        float[] outputTime = new float[heatmap.width * heatmap.height];
        float outOfBoundTime = 0f;
        Color[] outputCol = new Color[heatmap.width * heatmap.height];
        Color outofBoundCol;

        foreach (GazeSnapshot snapshot in _gazeData)
        {
            if (snapshot.timeStamp < from)
                continue;
            else if (snapshot.timeStamp > to)
                break;

            Vector3 dir = snapshot.headToBodyDir;
            //perspective projection, origin at bottom-left
            float projX = 0.5f * (dir.x / dir.z + halfProjWidth) / halfProjWidth;
            float projY = 0.5f * (dir.y / dir.z + halfProjHeight) / halfProjHeight;

            if (dir.z < 0f || projX < 0f || projX > 1f || projY < 0f || projY > 1f)
            {
                outOfBoundTime += updateInteval;
            }
            else
            {
                int mapX = Mathf.RoundToInt(projX * (float)(heatmap.width - 1));
                int mapY = Mathf.RoundToInt(projY * (float)(heatmap.height - 1));

                outputTime[mapX + mapY * heatmap.width] += updateInteval;
            }
        }

        maxTime = outputTime.Max();
        maxTime = Mathf.Max(maxTime, outOfBoundTime);
        for (int i = 0; i < heatmap.width * heatmap.height; ++i)
        {
            outputCol[i] = heatmapGradient.Evaluate
                (Mathf.Clamp01(heatmapColorCurve.Evaluate(outputTime[i] / maxTime)));
            outputCol[i].a = 0.5f;
        }
        outofBoundCol = heatmapGradient.Evaluate
            (Mathf.Clamp01(heatmapColorCurve.Evaluate(outOfBoundTime / maxTime)));
        outofBoundCol.a = 0.5f;
        heatmap.SetPixels(outputCol);
        heatmap.Apply();
    }

    //Call me to replay gaze trajectory with a given material
    //from: query start time (0 is the time when VR scene starts)
    //to: query end time
    //targetMat: where do you want to render the trajectory
    //if in Editor and assign null to targetMat, HeatmapTracker would perform a screen overlay effect
    //return: false when the tracker is replaying now.
    public bool Replay(float from, float to, Material targetMat)
    {
        if (_currReplayCR != null)
            return false;

        _currReplayCR = StartCoroutine(Replay_CR(from, to, targetMat));
        return true;

    }

    private IEnumerator Replay_CR(float from, float to, Material targetMat)
    {

        float halfProjHeight = Mathf.Tan(verticalFOVDeg * 0.5f * Mathf.Deg2Rad);
        float halfProjWidth = halfProjHeight * aspect;
        float length = to - from;
        Texture2D heatmap = new Texture2D(widthResolution, Mathf.RoundToInt((float)widthResolution / aspect),
                         TextureFormat.ARGB32, false);
        Color[] pixels = new Color[heatmap.width * heatmap.height];
        for (int i = 0; i < heatmap.width * heatmap.height; ++i)
        {
            pixels[i].r = 1f;
            pixels[i].g = 1f;
            pixels[i].b = 1f;
            pixels[i].a = 0.5f;
        }
        heatmap.SetPixels(pixels);
        heatmap.Apply();

        float[] outputTime = new float[heatmap.width * heatmap.height];
        float outOfBoundTime = 0f;

        List<GazeSnapshot> gazeDataCopy = new List<GazeSnapshot>(_gazeData);
        LinkedList<int> mapXs = new LinkedList<int>();
        LinkedList<int> mapYs = new LinkedList<int>();

        foreach (GazeSnapshot snapshot in gazeDataCopy)
        {
            if (snapshot.timeStamp < from)
                continue;
            else if (snapshot.timeStamp > to)
                break;

            Vector3 dir = snapshot.headToBodyDir;
            //perspective projection, origin at bottom-left
            float projX = 0.5f * (dir.x / dir.z + halfProjWidth) / halfProjWidth;
            float projY = 0.5f * (dir.y / dir.z + halfProjHeight) / halfProjHeight;

            if (dir.z < 0f || projX < 0f || projX > 1f || projY < 0f || projY > 1f)
            {
                outOfBoundTime += updateInteval;
            }
            else
            {
                int mapX = Mathf.RoundToInt(projX * (float)(heatmap.width - 1));
                int mapY = Mathf.RoundToInt(projY * (float)(heatmap.height - 1));

                outputTime[mapX + mapY * heatmap.width] += updateInteval;
                mapXs.AddLast(mapX);
                mapYs.AddLast(mapY);
            }
        }

        float maxTime = outputTime.Max();
        maxTime = Mathf.Max(maxTime, outOfBoundTime);
        for (int i = 0; i < outputTime.Length; ++i)
            outputTime[i] = 0f;

        if (targetMat == null)
        {

#if UNITY_EDITOR
            if (_overLayMat == null)
                _overLayMat = new Material(Shader.Find("VR_Rehearsal_app/ScreenOverlay"));
            _overLayMat.mainTexture = heatmap;
            _replayOverlay = true;
#else
            _currReplayCR = null;
            return;
#endif
        }

        while (mapXs.Count > 0)
        {
            outputTime[mapXs.First.Value + mapYs.First.Value * heatmap.width] += updateInteval;
            //Color col = heatmapGradient.Evaluate(1f);
            Color col = heatmapGradient.Evaluate(Mathf.Clamp01(heatmapColorCurve.Evaluate
                (outputTime[mapXs.First.Value + mapYs.First.Value * heatmap.width] / maxTime)));
            col.a = 0.5f;
            heatmap.SetPixel(mapXs.First.Value, mapYs.First.Value, col);
            heatmap.Apply();
            mapXs.RemoveFirst();
            mapYs.RemoveFirst();
            yield return new WaitForSeconds(updateInteval / replaySpeed);
        }

#if UNITY_EDITOR
        _replayOverlay = false;
#endif

        if (OnReplayFinish != null)
            OnReplayFinish();

        _currReplayCR = null;
    }

    //Replay sample usage

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.R))
    //        Replay(0f, Time.time, null);
    //}

#if UNITY_EDITOR
        //screen overlay routine in editor ONLY
        //need to be plugged to Cardboard post render object
    public void RenderOverlay()
    {
        if (!_replayOverlay)
            return;
   
        GL.PushMatrix();
        GL.LoadOrtho();

        _overLayMat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.TexCoord(new Vector3(0, 0, 0));
        GL.Vertex3(0, 0, 0);

        GL.TexCoord(new Vector3(1, 0, 0));
        GL.Vertex3(1, 0, 0);

        GL.TexCoord(new Vector3(1, 1, 0));
        GL.Vertex3(1, 1, 0);

        GL.TexCoord(new Vector3(0, 1, 0));
        GL.Vertex3(0, 1, 0);

        GL.End();

        GL.PopMatrix();
    }
#endif
}
