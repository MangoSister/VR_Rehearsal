/* HeatmapGenerator.cs
 * Yang Zhou, last modified on Mar 3, 2016
 * HeatmapGenerator generates heatmap based on the data from HeatmapTracker.
 * Dependencies: data and params from HeatmapTracker
 * May implement interpolation in the future
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class HeatmapGenerator : MonoBehaviour
{
    //The color pattern of heatmap
    public Gradient heatmapGradient;

    //The curve used to map staring time (relative to query interval) to gradient color
    //instead of simple linear lerp, a curve provides more flexibility and more reasonable mapping
    public AnimationCurve heatmapColorCurve;

    //Should get params from HeatmapTracker
    public float verticalFOVDeg = 60f;

    //The width of generated heatmap. The height of the map depends on aspect
    public int widthResolution = 64;

    //Shoulld get params from HeatmapTracker
    public float aspect;

    private void Start()
    {
        verticalFOVDeg = Mathf.Clamp(PresentationData.out_HGVertFOVDeg, 0f, 179f);
        aspect = Mathf.Clamp(PresentationData.out_HGAspect, 0.01f, 100f);
    }

    //Call me to query a time period and generate the heatmap (as well as the max staring time of all directions)
    //from: query start time (0 is the time when VR scene starts)
    //to: query end time
    //heatmap: generated heatmap
    //maxTime: the max staring time (of one direction) among all directions
    public void GenerateMap(List<GazeSnapshot> gazeData, float from, float to, out Texture2D heatmap, out float maxElementTime)
    {
        if (gazeData == null || gazeData.Count == 0)
        {
            heatmap = null;
            maxElementTime = 0f;
            return;
        }

        from = Mathf.Clamp(from, PresentationData.in_EnterTime, PresentationData.out_ExitTime);
        to = Mathf.Clamp(to, PresentationData.in_EnterTime, PresentationData.out_ExitTime);
        float halfProjHeight = Mathf.Tan(verticalFOVDeg * 0.5f * Mathf.Deg2Rad);
        float halfProjWidth = halfProjHeight * aspect;
        float length = to - from;


        heatmap = new Texture2D(widthResolution, Mathf.RoundToInt((float)widthResolution / aspect),
                                 TextureFormat.ARGB32, false);
        float[] outputTime = new float[heatmap.width * heatmap.height];
        float outOfBoundTime = 0f;
        Color[] outputCol = new Color[heatmap.width * heatmap.height];
        Color outofBoundCol;

        float lastTime = gazeData[0].timeStamp;
        foreach (GazeSnapshot snapshot in gazeData)
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
                outOfBoundTime += (snapshot.timeStamp - lastTime);
            }
            else
            {
                int mapX = Mathf.RoundToInt(projX * (float)(heatmap.width - 1));
                int mapY = Mathf.RoundToInt(projY * (float)(heatmap.height - 1));

                outputTime[mapX + mapY * heatmap.width] += (snapshot.timeStamp - lastTime);
            }

            lastTime = snapshot.timeStamp;
        }

        maxElementTime = outputTime.Max();
        maxElementTime = Mathf.Max(maxElementTime, outOfBoundTime);
        for (int i = 0; i < heatmap.width * heatmap.height; ++i)
        {
            outputCol[i] = heatmapGradient.Evaluate
                (Mathf.Clamp01(heatmapColorCurve.Evaluate(outputTime[i] / maxElementTime)));
            outputCol[i].a = 0.5f;
        }
        outofBoundCol = heatmapGradient.Evaluate
            (Mathf.Clamp01(heatmapColorCurve.Evaluate(outOfBoundTime / maxElementTime)));
        outofBoundCol.a = 0.5f;
        heatmap.SetPixels(outputCol);
        heatmap.Apply();
    }

    public void GenerateMapToFile(List<GazeSnapshot> gazeData, float from, float to, string path, out float maxTime)
    {
        Texture2D heatmap;
        GenerateMap(gazeData, from, to, out heatmap, out maxTime);
        File.WriteAllBytes(path, heatmap.EncodeToPNG());
    }

    //sample usage
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.R))
    //    {
    //        float maxTime;
    //        GenerateMapToFile(PresentationData.out_HGGazeData, 0, Time.time,
    //            Path.Combine(Application.persistentDataPath, "heatmap.png"), out maxTime);
    //    }
    //}
}
