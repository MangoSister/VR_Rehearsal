using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class GlobalObjManager
{
    private static readonly string _PRELOAD_SCENE_NAME = "sc_preload";
    private static readonly string _PREP_SCENE_NAME = "sc_UI";

    private static List<string> _presentSceneNames = new List<string> { "sc_present_0" };

    // global-wide monohehaviour, make sure they are in preload scene
    public static GlobalBehaviorCleaner globalBehaviorCleaner;
    public static GlobalBehaviorTest globalBehaviorTest;
    public static VRSceneTransition screenTransition;
    public static DownloadManager downloadManager;

    static GlobalObjManager()
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

    public static void LaunchPresentationScene(PresentationInitParam param)
    {
        if (SceneManager.GetActiveScene().name == _PREP_SCENE_NAME)
            SceneManager.LoadScene(param.sceneName);
            //Application.LoadLevel(param.sceneName);
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

public class PresentationInitParam
{
    public string sceneName;

    public PresentationInitParam(string name)
    {
        sceneName = name;
    }
}