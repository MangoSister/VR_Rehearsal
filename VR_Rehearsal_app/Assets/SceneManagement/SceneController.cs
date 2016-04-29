using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EnvType = PresentationData.EnvType;
using SoundCollection = AudioManager.SoundCollection;

public class SceneController : MonoBehaviour
{
    private static SceneController _currRoom = null;
    public static SceneController currRoom
    {
        get
        {
            if (_currRoom == null)
                _currRoom = FindObjectOfType<SceneController>();
            return _currRoom;
        }
    }

    //Lightmap & Lightprbes setting
    public struct EnvInfo
    {
        public string lightmapNearPath;
        public string lightmapFarPath;
        public string lightProbesPath;
        public string crowdConfigPath;
        public EnvInfo(string near, string far, string probes, string crowdConfig)
        { this.lightmapNearPath = near; this.lightmapFarPath = far; this.lightProbesPath = probes; this.crowdConfigPath = crowdConfig; }
    }

    public static Dictionary<EnvType, EnvInfo> EnvInfoDict = new Dictionary<EnvType, EnvInfo>
    {
        { EnvType.RPIS, new EnvInfo(null, "Lightmap-0_comp_light_RPIS", "lightprobes_RPIS", "rpis_chair") },
        { EnvType.ConferenceRoom, new EnvInfo(null, "Lightmap-0_comp_light_CONF", "lightprobes_CONF", "conferenceRoom_chair") },
        { EnvType.EmptySpace, new EnvInfo(null, "Lightmap-0_comp_light_EMPTY", "lightprobes_EMPTY", "rpis_chair") }
    };

    [Serializable]
    public class EnvDict : SerializableDictionary<EnvType, GameObject>
    {
        public EnvDict() : base() { }
        public EnvDict(IDictionary<EnvType, GameObject> dict) : base(dict) { }
    }

    [SerializeField, HideInInspector]
    public EnvDict envPrefabs = new EnvDict();

    //[HideInInspector, SerializeField]
    public bool overrideEnv;
    //[HideInInspector, SerializeField]
    public EnvType overrideEnvType;

    public GameObject presenter;
    public Transform presenterHead;
    public GameObject tutorialPage;
    public GameObject exitNotice;

    private GameObject _packedEnv;
    public SlidesPlayer slidesPlayer;
    public CrowdSimulator crowdSim;
    public HeatmapTracker heatmapTracker;
    public RecordingWrapper recordWrapper;
    public AudioManager audioManager;
    public InputManager inputManager;
    public ClockTimer timer;
    public Tutorial_PptKaraoke tutManager;

    private AudioUnit _ambientUnit = null;

    void Start ()
    {
        if (GlobalManager.screenTransition != null)
            GlobalManager.screenTransition.Fade(true, 1.0f);

#if UNITY_EDITOR
        if (overrideEnv)
            PresentationData.in_EnvType = overrideEnvType;
#endif

        LoadEnv();
        LoadLightProbes(Path.Combine("Lightmaps", EnvInfoDict[PresentationData.in_EnvType].lightProbesPath));
        crowdSim.Init();

        recordWrapper.Init();

        audioManager.Init();
        audioManager.earphonePlugged = recordWrapper.EarphonePlugged();
        if (audioManager.earphonePlugged)
        {
            audioManager.AllocateRand3dSound(SoundCollection.Ambient, audioManager.room.transform, Vector3.zero, out _ambientUnit);
            _ambientUnit.source.volume = audioManager.ambientVolume * UnityEngine.Random.Range(0.8f, 1.2f);
            _ambientUnit.source.loop = true;
            _ambientUnit.Play();
        }

        inputManager.OnPracticeBegin += BeginPractice;
        exitNotice.SetActive(false);
    }

    private void LoadEnv()
    {
        _packedEnv = Instantiate(envPrefabs[PresentationData.in_EnvType]);

        inputManager.player = _packedEnv.transform.GetComponentInChildren<SlidesPlayer>();
        inputManager.OnPracticeEnd += EndPractice;
        inputManager.OnExitVRScene += ExitEnv;

        timer = _packedEnv.transform.GetComponentInChildren<ClockTimer>();
        timer.SetMaxTime((int)PresentationData.in_ExpectedTime);
        
        crowdSim.crowdConfigFileName = EnvInfoDict[PresentationData.in_EnvType].crowdConfigPath;
        crowdSim.crowdParent = _packedEnv.transform.Find("CrowdParentTransform");
        recordWrapper.debugText = _packedEnv.transform.Find("RecordDebugText").GetComponent<TextMesh>();
        slidesPlayer = _packedEnv.transform.GetComponentInChildren<SlidesPlayer>();
        audioManager.room = _packedEnv.transform.GetComponentInChildren<CardboardAudioRoom>();
        audioManager.miscBound = _packedEnv.transform.GetComponentInChildren<AudioBound>();
        //tutManager.slidePlayer = inputManager.GetComponent<SlidesPlayer>();
        //tutManager.timerPlayer = env.transform.GetComponentInChildren<clockTimer>();
    }

    private void BeginPractice()
    {
        PresentationData.in_EnterTime = Time.time;

        crowdSim.StartSimulation();
        heatmapTracker.StartTrack();
        recordWrapper.StartRecording();
        if (_ambientUnit != null)
            _ambientUnit.StopFadeAndRecycle(1.0f);
        if (audioManager.earphonePlugged)
            audioManager.StartMiscSound();
        tutorialPage.SetActive(false);
        timer.StartCounting();
    }

    private void EndPractice()
    {
        PresentationData.out_ExitTime = Time.time;

        crowdSim.StopSimulation();
        heatmapTracker.StopTrack();
        recordWrapper.EndRecording();
        if (audioManager.earphonePlugged)
            audioManager.StopMiscSound();

        _packedEnv.SetActive(false);
        exitNotice.SetActive(true);
        Camera.main.backgroundColor = Color.white;
        Camera.main.GetComponent<StereoController>().UpdateStereoValues();
    }

    public void ExitEnv()
    {
        StartCoroutine(ExitEnv_CR());
    }

    private IEnumerator ExitEnv_CR()
    {
        yield return new WaitForSeconds(0.5f);
        GlobalManager.EndPresentation
        (
            heatmapTracker.verticalFOVDeg,
            heatmapTracker.aspect,
            heatmapTracker.output,
            heatmapTracker.scn,
            inputManager.outputTransitionRecord,
            slidesPlayer._slides,
            recordWrapper.recordingFilePath,
            recordWrapper.outputFluencyRecord
        );
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            EndPractice();
            ExitEnv();
        }
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

                    //if (LightmapSettings.lightProbes == null)
                    //{
                    LightProbes probes = Instantiate(LightmapSettings.lightProbes) as LightProbes;
                    probes.name = "Imported probes";
                    probes.bakedProbes = bakedProbes;
                    LightmapSettings.lightProbes = probes;
                    //}
                    //LightmapSettings.lightProbes.bakedProbes = bakedProbes;
                    //LightmapSettings.lightProbes = new LightProbes();
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
