using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(AudienceAnimWarehouse))]
public class AudienceAnimWarehouseInspector : Editor
{
    private AudienceAnimWarehouse warehouse { get { return target as AudienceAnimWarehouse; } }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorHelper.ShowArray(ref warehouse.basicFocusedClips, "Basic Focused Clips", ShowProbabilitySingleClip);
        EditorHelper.ShowArray(ref warehouse.basicBoredClips, "Basic Bored Clips", ShowProbabilitySingleClip);
        EditorHelper.ShowArray(ref warehouse.basicChattingClips, "Basic Chatting Clips", ShowProbabilityClipPair);
        EditorHelper.ShowArray(ref warehouse.followFocusedClips, "Follow Focused Clips", ShowProbabilitySingleClip);
        EditorHelper.ShowArray(ref warehouse.followBoredClips, "Follow Bored Clips", ShowProbabilitySingleClip);
        EditorHelper.ShowArray(ref warehouse.followChattingClips, "Follow Chatting Clips", ShowProbabilityClipPair);
        serializedObject.ApplyModifiedProperties();

    }

    private void ShowProbabilitySingleClip(ref ProbabilitySingleClip pclip)
    {
        if (pclip == null)
            pclip = new ProbabilitySingleClip();
        EditorGUILayout.BeginHorizontal();
        pclip.clip = EditorGUILayout.ObjectField(pclip.clip, typeof(AnimationClip), false) as AnimationClip;
        pclip.probability = EditorGUILayout.Slider(pclip.probability, 0f, 1f);
        EditorGUILayout.EndHorizontal();
    }

    private void ShowProbabilityClipPair(ref ProbabilityClipPair pair)
    {
        if (pair == null)
            pair = new ProbabilityClipPair();
        EditorGUILayout.BeginHorizontal();
        pair.clip1 = EditorGUILayout.ObjectField(pair.clip1, typeof(AnimationClip), false) as AnimationClip;
        pair.clip2 = EditorGUILayout.ObjectField(pair.clip2, typeof(AnimationClip), false) as AnimationClip;
        pair.probability = EditorGUILayout.Slider(pair.probability, 0f, 1f);
        EditorGUILayout.EndHorizontal();
    }
}