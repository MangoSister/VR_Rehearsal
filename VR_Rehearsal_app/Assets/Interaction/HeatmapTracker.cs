using UnityEngine;
using System.Collections;

public class HeatmapTracker : MonoBehaviour
{
    public float verticalFOVDeg = 60f;
    public float updateInteval = 1f;

    public int widthResolution = 256;
    private float[,] _heatmap;
    private float _outofBoundRecord = 0f;
    private float[,] heatmap
    {
        get
        {
            if (_heatmap == null)
                _heatmap = new float[widthResolution, Mathf.RoundToInt((float)widthResolution / aspect)];
            return _heatmap;
        }
    }
    private int mapWidth { get { return heatmap.GetLength(0); } }
    private int mapHeight { get { return heatmap.GetLength(1); } }
    private float aspect
    { get { return (float)Screen.width / (float)Screen.height; } }

    private Texture2D _visual;
    private Texture2D visual
    {
        get
        {
            if (_visual == null)
            {
                _visual = new Texture2D(widthResolution, Mathf.RoundToInt((float)widthResolution / aspect),
                                        TextureFormat.ARGB32, false);
                Color[] pixels = new Color[_visual.width * _visual.height];
                for (int i = 0; i < _visual.width * _visual.height; ++i)
                {
                    pixels[i].r = 1f;
                    pixels[i].g = 1f;
                    pixels[i].b = 1f;
                    pixels[i].a = 0.5f;
                }
                _visual.SetPixels(pixels);
                _visual.Apply();
            }
            return _visual;
        }
    }
    private Material _overLayMat;
    private void Start()
    {
        verticalFOVDeg = Mathf.Clamp(verticalFOVDeg, 1f, 179f);
    }

    private IEnumerator Update_CR()
    {
        var head = RoomCenter.currRoom.presenterHead;
        var body = RoomCenter.currRoom.presenter.transform;
        float halfProjHeight = Mathf.Tan(verticalFOVDeg * 0.5f);
        float halfProjWidth = halfProjHeight * aspect;
        while (true)
        {
            float z = Vector3.Dot(head.forward, body.forward);
            if (z < 0f)
            {
                //out of "view"
                _outofBoundRecord += updateInteval;
                yield return new WaitForSeconds(updateInteval);
                continue;
            }
            float x = Vector3.Dot(head.forward, body.right);
            float y = Vector3.Dot(head.forward, body.up);

            //origin at top-left
            float projX = 0.5f * (x * z + halfProjWidth) / halfProjWidth;
            float projY = 0.5f * (y * z + halfProjHeight) / halfProjHeight;
            if (projX < 0f || projX > 1f || projY < 0f || projY > 1f)
            {
                //out of "view"
                _outofBoundRecord += updateInteval;
                yield return new WaitForSeconds(updateInteval);
                continue;
            }

            int mapX = Mathf.RoundToInt(projX * (float)(mapWidth - 1));
            int mapY = Mathf.RoundToInt(projY * (float)(mapHeight - 1));

            heatmap[mapX, mapY] += updateInteval;

#if UNITY_EDITOR
            Color col = _visual.GetPixel(mapX, mapY);
            col.r = Mathf.Clamp01((col.r * 255.0f - updateInteval) / 255.0f);
            col.b = Mathf.Clamp01((col.b * 255.0f - updateInteval) / 255.0f);
            _visual.SetPixel(mapX, mapY, col);
            _visual.Apply();
#endif
            yield return new WaitForSeconds(updateInteval);
        }
    }

    private void OnEnable()
    {
        if (RoomCenter.currRoom.presenterHead != null)
            StartCoroutine(Update_CR());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

#if UNITY_EDITOR
    private void OnPostRender()
    {
        if (_overLayMat == null)
            _overLayMat = new Material(Shader.Find("VR_Rehearsal_app/ScreenOverLay"));
   
        GL.PushMatrix();
        GL.LoadOrtho();

        _overLayMat.mainTexture = visual;
        _overLayMat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.Vertex3(0, 1, 0);

        GL.End();

        GL.PopMatrix();
    }
#endif
}
