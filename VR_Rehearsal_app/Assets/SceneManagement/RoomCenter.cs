using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.IO;

public class RoomCenter : MonoBehaviour
{
    private static RoomCenter _currRoom = null;
    public static RoomCenter currRoom
    {
        get
        {
            if (_currRoom == null)
                _currRoom = FindObjectOfType<RoomCenter>();
            return _currRoom;
        }
    }

    public Transform presentDest;
    public Transform roomDoorIn;
    public Transform roomDoorOut;

    public GameObject presenter;
    public Transform presenterHead;

    public HeatmapTracker heatmapTracker;
    public RecordingWrapper recordWrapper;
    public SlidesPlayerCtrl slidesPlayerCtrl;
    public ExitTrigger exitTrigger;

    private AudioUnit _ambientUnit = null;

    void Start ()
    {
#if UNITY_ANDROID
     Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif 

        LoadLights();
        if (GlobalManager.screenTransition != null)
            GlobalManager.screenTransition.Fade(true, 1.0f);

        recordWrapper.StartRecording();

        OperateAmbient(true);
        StartCoroutine(SilenceAfterOpenning_CR());
    }

    private void LoadLights()
    {
        //load lightmap
        PresentationData.LightingInfo info = PresentationData.lightingInfoDict[PresentationData.in_EnvType];
        LightmapData data = new LightmapData();
        if (info.far != null)
        {
            data.lightmapFar = Resources.Load<Texture2D>
                (Path.Combine(GlobalManager._PRESENT_SCENE_NAME, info.far));
        }

        if (info.near != null)
        {
            data.lightmapNear = Resources.Load<Texture2D>
                (Path.Combine(GlobalManager._PRESENT_SCENE_NAME, info.near));
        }

        LightmapSettings.lightmaps = new LightmapData[] { data };

        //load light probe
        LoadLightProbes(Path.Combine(GlobalManager._PRESENT_SCENE_NAME, info.probes));
        
    }

    private void OperateAmbient(bool enable)
    {
        if (enable)
            AudioManager.currAudioManager.Play3dSound("Unrest", 1.0f, transform, Vector3.zero, 2.0f, true, ref _ambientUnit);
        else
        {
            _ambientUnit.Stop(2.0f);
            _ambientUnit = null;
        }
    }

    //private void OperateAmbient(DestType dest)
    //{
    //    StopAllCoroutines();
    //    if (dest == DestType.PODIUM)
    //        StartCoroutine(SilenceAfterOpenning_CR());
    //    else if (dest == DestType.DOOR_OUT)
    //        OperateAmbient(true);
    //}


    private IEnumerator SilenceAfterOpenning_CR()
    {
        yield return new WaitForSeconds(5f);
        OperateAmbient(false);
    }

    public void EndPresentation()
    {
        recordWrapper.EndRecording();
#if UNITY_ANDROID
        Screen.orientation = ScreenOrientation.AutoRotation;
#endif
        GlobalManager.EndPresentation
            (
                heatmapTracker.verticalFOVDeg,
                heatmapTracker.aspect,
                heatmapTracker.output,
                heatmapTracker.scn,
                slidesPlayerCtrl.outputTransitionRecord
            );
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            EndPresentation();
    }
#endif

    private void LoadLightProbes(string path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            TextAsset data = Resources.Load<TextAsset>(path);
            using (var stream = new MemoryStream(data.bytes))
            {
                using (var input = new BinaryReader(stream))
                {
                    int count = input.ReadInt32();
                    SphericalHarmonicsL2[] bakedProbes = new SphericalHarmonicsL2[count];
                    for (int i = 0; i < count; ++i)
                    {
                        bakedProbes[i].Clear();
                        for (int ch = 0; ch < 3; ++ch)
                            for (int coef = 0; coef < 9; ++coef)
                                bakedProbes[i][ch, coef] = input.ReadSingle();
                    }

                    if (LightmapSettings.lightProbes == null)
                    {
                        LightProbes probes = new LightProbes();
                        probes.name = "Imported probes";
                        LightmapSettings.lightProbes = probes;
                    }
                    LightmapSettings.lightProbes.bakedProbes = bakedProbes;
                    
                    input.Close();
                }
                stream.Close();
            }

            Resources.UnloadAsset(data);
        }
        else LoadLightProbeFromScene();
    }

    private void LoadLightProbeFromScene()
    {
        Light[] lights = FindObjectsOfType<Light>();
        Color ambient = RenderSettings.ambientLight * RenderSettings.ambientIntensity;

        SphericalHarmonicsL2[] bakedProbes = LightmapSettings.lightProbes.bakedProbes;
        Vector3[] probePositions = LightmapSettings.lightProbes.positions;
        int probeCount = LightmapSettings.lightProbes.count;

        // Clear all probes
        for (int i = 0; i < probeCount; i++)
            bakedProbes[i].Clear();

        //// Add ambient light to all probes
        //for (int i = 0; i < probeCount; i++)
        //    bakedProbes[i].AddAmbientLight(ambient);

        // Add directional and point lights' contribution to all probes
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                for (int i = 0; i < probeCount; i++)
                    bakedProbes[i].AddDirectionalLight(-l.transform.forward, l.color, l.intensity);
            }
            else if (l.type == LightType.Point)
            {
                for (int i = 0; i < probeCount; i++)
                    SHAddPointLight(probePositions[i], l.transform.position, l.range, l.color, l.intensity, ref bakedProbes[i]);
            }
        }

        LightmapSettings.lightProbes.bakedProbes = bakedProbes;
    }

    private void SHAddPointLight(Vector3 probePosition, Vector3 position, float range, Color color, float intensity, ref SphericalHarmonicsL2 sh)
    {
        // From the point of view of an SH probe, point light looks no different than a directional light,
        // just attenuated and coming from the right direction.
        Vector3 probeToLight = position - probePosition;
        float attenuation = 1.0F / (1.0F + 25.0f * probeToLight.sqrMagnitude / (range * range));
        sh.AddDirectionalLight(probeToLight.normalized, color, intensity * attenuation);
    }
}
