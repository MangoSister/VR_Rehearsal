#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using EnvType = PresentationData.EnvType;

[CustomEditor(typeof(SceneController))]
public class SceneControllerInspector : Editor
{
    private SceneController ctrl { get { return (target as SceneController); } }
    private EnvType tmpType;
    private GameObject tmpObj;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        SerializableDictionary<EnvType, GameObject> copy;
        if (ctrl.envPrefabs != null)
            copy = new SerializableDictionary<EnvType, GameObject>(ctrl.envPrefabs);
        else
            copy = new SerializableDictionary<EnvType, GameObject>();
        EditorHelper.ShowEnumDict(ref copy, ref tmpType, ref tmpObj, "Environments");
        ctrl.envPrefabs = new SceneController.EnvDict(copy);

        //ctrl.overrideEnv = EditorGUILayout.BeginToggleGroup("Override Env Type?", ctrl.overrideEnv);
        //ctrl.overrideEnvType = (EnvType)EditorGUILayout.EnumPopup("Type", ctrl.overrideEnvType);
        GUI.enabled = false;
        EditorGUILayout.TextField("Crowd Config File Name", SceneController.EnvInfoDict[ctrl.overrideEnvType].crowdConfigPath);
        GUI.enabled = true;
        //EditorGUILayout.EndToggleGroup();

        serializedObject.ApplyModifiedProperties();
    }
}

#endif