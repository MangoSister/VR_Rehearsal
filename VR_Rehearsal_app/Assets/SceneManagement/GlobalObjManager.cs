using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class GlobalManager
{
    public static readonly string _PRELOAD_SCENE_NAME = "sc_preload";
    public static readonly string _PREP_SCENE_NAME = "sc_UI";
    public static readonly string _PRESENT_SCENE_NAME = "sc_present_0";
    public static readonly string _EVAL_SCENE_NAME = "sc_evalutation";

    // global-wide monohehaviour, make sure they are in preload scene
    public static GlobalBehaviorCleaner globalBehaviorCleaner;
    public static GlobalBehaviorTest globalBehaviorTest;
    public static VRSceneTransition screenTransition;
    public static DownloadManager downloadManager;

    static GlobalManager()
    {
        globalBehaviorCleaner = FindGlobalBehavior<GlobalBehaviorCleaner>();
        globalBehaviorTest = FindGlobalBehavior<GlobalBehaviorTest>();
        screenTransition = FindGlobalBehavior<VRSceneTransition>();
        downloadManager = FindGlobalBehavior<DownloadManager>();
    }

    private static T FindGlobalBehavior<T>() where T : GlobalBehaviorBase
    {
        T gBhv = GameObject.FindObjectOfType<T>();
        if (gBhv != null)
            return gBhv;
        else
        {
#if UNITY_EDITOR
            Debug.Log("failed to find global behavior");
#endif
            //throw new UnityException("failed to find global behavior");
            return null;
        }
    }

    public static void EnterPresentation
        (
            PresentationData.EnvType envType = PresentationData.EnvType.RPIS
        )
    {
        PresentationData.in_EnvType = envType;

        if (SceneManager.GetActiveScene().name == _PREP_SCENE_NAME)
            SceneManager.LoadScene(_PRESENT_SCENE_NAME);
            //Application.LoadLevel(param.sceneName);
    }

    public static void EndPresentation
        (
            float HGVertFOVDeg,
            float HGAspect,
            List<GazeSnapshot> HGGazeData,
            Texture2D screenshot
        )
    {
        PresentationData.out_HGVertFOVDeg = HGVertFOVDeg;
        PresentationData.out_HGAspect = HGAspect;
        PresentationData.out_HGGazeData = HGGazeData;
        PresentationData.out_Screenshot = screenshot;

        if (SceneManager.GetActiveScene().name == _PRESENT_SCENE_NAME)
            SceneManager.LoadScene(_EVAL_SCENE_NAME);
    }

    public static void LaunchPreparationScene()
    {
        if (SceneManager.GetActiveScene().name != _PREP_SCENE_NAME)
            SceneManager.LoadScene(_PREP_SCENE_NAME);
            //Application.LoadLevel(_PREP_SCENE_NAME);
    }

    public static void Init()
    {
#if UNITY_EDITOR
        Debug.Log("SceneManager Initialized");
#endif
#if UNITY_EDITOR
        //Application.LoadLevel(System.IO.Path.GetFileNameWithoutExtension(EditorPrefs.GetString("SceneAutoLoader.PreviousScene")));
        SceneManager.LoadScene(System.IO.Path.GetFileNameWithoutExtension(EditorPrefs.GetString("SceneAutoLoader.PreviousScene")));
#else
        Application.LoadLevel("sc_UI");
        SceneManager.LoadScene("sc_UI");
#endif
    }

}

public static class PresentationData
{
    public enum EnvType
    {
        RPIS,
    }

    public struct LightingInfo
    {
        public string near;
        public string far;
        public string probes;
        public LightingInfo(string near, string far, string probes)
        { this.near = near; this.far = far; this.probes = probes; }
    }

    public static Dictionary<EnvType, LightingInfo> lightingInfoDict = new Dictionary<EnvType, LightingInfo>
    {
        { EnvType.RPIS, new LightingInfo(null, "Lightmap-0_comp_light_1", "lightprobes") }
    };

    //Input
    public static EnvType in_EnvType;

    //Output
    public static float out_HGVertFOVDeg;
    public static float out_HGAspect;
    public static List<GazeSnapshot> out_HGGazeData;
    public static Texture2D out_Screenshot;
}