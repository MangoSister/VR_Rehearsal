/* GlobalObjManager.cs
 * Yang Zhou, last modified on Mar 3, 2016
 * GlobalObjManager is a static class that manages gloabl monobehaviors (those that stay alive between scenes)
 * by creating a separate preloading scene to first initialize them and pass them to the following
 * scenes.

 * It also manages data transfering between scene (mostly for presentation scene now).
 * NOTICE: Use the interface provided instead of Unity built-in scene loading functions

 * Dependencies: All global monobehaviors should inherit GlobalBehaviorBase, and be placed ONLY IN preload scene.
 * Dependencies: SceneAutoLoader (in Editor only) to 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class GlobalManager
{
    //fixed scene names
    public static readonly string _PRELOAD_SCENE_NAME = "sc_preload";
    public static readonly string _PREP_SCENE_NAME = "sc_UI";
    public static readonly string _PRESENT_SCENE_NAME = "sc_present_0";
    public static readonly string _EVAL_SCENE_NAME = "sc_evaluationNew";

    // global-wide monohehaviour, make sure they are in preload scene
    public static GlobalBehaviorCleaner globalBehaviorCleaner;
    public static GlobalBehaviorTest globalBehaviorTest;
    public static VRSceneTransition screenTransition;
 //   public static DownloadManager downloadManager;

    static GlobalManager()
    {
        globalBehaviorCleaner = FindGlobalBehavior<GlobalBehaviorCleaner>();
        globalBehaviorTest = FindGlobalBehavior<GlobalBehaviorTest>();
        screenTransition = FindGlobalBehavior<VRSceneTransition>();
    //    downloadManager = FindGlobalBehavior<DownloadManager>();
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

    /*SHIBA*/
    public static void EnterPresentation()
    {
        PresentationData.in_EnterTime = Time.time;

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        //if (SceneManager.GetActiveScene().name == _PREP_SCENE_NAME)
        SceneManager.LoadScene(_PRESENT_SCENE_NAME);
        //Application.LoadLevel(param.sceneName);
    }

    //Use me to enter presentation scene!!!
    //Filling the input data area!!
    public static void EnterPresentation
        (
            string slidePath,
            float expectedTime,
            int voiceThreshold,
            PresentationData.EnvType envType = PresentationData.EnvType.RPIS
        )
    {
        PresentationData.in_SlidePath = slidePath;
        PresentationData.in_ExpectedTime = expectedTime;
        PresentationData.in_VoiceThreshold = voiceThreshold;
        PresentationData.in_EnvType = envType;
        
        PresentationData.in_EnterTime = Time.time;

        if (SceneManager.GetActiveScene().name == _PREP_SCENE_NAME)
        {
            ApplicationChrome.statusBarState = ApplicationChrome.States.Hidden;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            SceneManager.LoadScene(_PRESENT_SCENE_NAME);
        }
            //Application.LoadLevel(param.sceneName);
    }

    //Use me to end presentation scene!!!
    //Filling the output data area!!
    public static void EndPresentation
        (
            float HGVertFOVDeg,
            float HGAspect,
            List<GazeSnapshot> HGGazeData,
            Texture2D screenshot,
            List<KeyValuePair<float, int>> transitionRecord,
            List<Texture2D> slides,
            string recordingFilePath,
            List<KeyValuePair<bool, int>> fluencyRecord
        )
    {
        PresentationData.out_HGVertFOVDeg = HGVertFOVDeg;
        PresentationData.out_HGAspect = HGAspect;
        PresentationData.out_HGGazeData = HGGazeData;
        PresentationData.out_Screenshot = screenshot;
        PresentationData.out_SlidesTransitionRecord = transitionRecord;
        PresentationData.out_Slides = slides;
        PresentationData.out_RecordingFilePath = recordingFilePath;
        PresentationData.out_FluencyRecord = fluencyRecord;

        if (SceneManager.GetActiveScene().name == _PRESENT_SCENE_NAME)
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;


            SceneManager.LoadScene(_EVAL_SCENE_NAME);
        }
    }

    //Use me to enter preparation scene (normal 2d mobile scene)!!
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

/* PresentationData
 * A static area to save shared data
 * Currently it is only for presentation scene (input and output)
 */
public static class PresentationData
{
    public enum EnvType
    {
        RPIS,
        ConferenceRoom,
        EmptySpace,
    }

    //Input
    public static EnvType in_EnvType = EnvType.RPIS;
    public static string in_SlidePath;
    public static float in_EnterTime = 0f;
    public static float in_ExpectedTime = 600f;
    public static int in_VoiceThreshold;
    public static bool in_VoiceEcho;

    //Output
    public static float out_ExitTime;
    public static float out_HGVertFOVDeg;
    public static float out_HGAspect;
    public static List<GazeSnapshot> out_HGGazeData;
    public static Texture2D out_Screenshot;
    public static List<KeyValuePair<float, int>> out_SlidesTransitionRecord;
    public static List<Texture2D> out_Slides;
    public static string out_RecordingFilePath;
    public static List<KeyValuePair<bool, int>> out_FluencyRecord;

}