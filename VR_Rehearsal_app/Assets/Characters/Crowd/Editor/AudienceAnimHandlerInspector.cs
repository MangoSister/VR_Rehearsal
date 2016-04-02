#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using States = Audience.States;

[CustomEditor(typeof(AudienceAnimHandlerBasic), true)]
[CanEditMultipleObjects]
public class AudienceAnimHandlerInspector : Editor
{
    private enum Level
    {
        Basic,
        Follow,
        Full
    };

    private AudienceAnimHandlerBasic anim { get { return target as AudienceAnimHandlerBasic; } }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(anim), typeof(MonoScript), false);
        GUI.enabled = true;

        Level lv;
        if (anim is AudienceAnimHandlerFull)
            lv = Level.Full;
        else if (anim is AudienceAnimHandlerFollow)
            lv = Level.Follow;
        else lv = Level.Basic;

        EditorGUILayout.LabelField(string.Format("Level: {0}", lv), EditorStyles.boldLabel);
        anim.controller = EditorGUILayout.ObjectField("Anim Ctrl", anim.controller, typeof(Animator), true) as Animator;
        anim.eyeIcon = EditorGUILayout.ObjectField("Eye Icon", anim.eyeIcon, typeof(GameObject), true) as GameObject;

        if (lv != Level.Basic)
        {
            var anim_follow = anim as AudienceAnimHandlerFollow;
            anim_follow.LerpAnimLayerSpeed = EditorGUILayout.Slider("Neck Layer Lerp Speed", anim_follow.LerpAnimLayerSpeed, 0.2f, 5f);
            anim_follow.SwitchFollowDegSpeed = EditorGUILayout.Slider("Neck Follow Speed (degree)", anim_follow.SwitchFollowDegSpeed, 60f, 720);
        }

        if (lv == Level.Full)
        {
            var anim_full = anim as AudienceAnimHandlerFull;
            anim_full.repeatPeriodBound.x = EditorGUILayout.FloatField("Repeat Period Low", anim_full.repeatPeriodBound.x);
            anim_full.repeatPeriodBound.y = EditorGUILayout.FloatField("Repeat Period High", anim_full.repeatPeriodBound.y);
        }



        EditorGUILayout.Separator();

        if (anim.controller != null)
        {
            anim.isManualCtrl = EditorGUILayout.BeginToggleGroup("Manual Control Parameters", anim.isManualCtrl);

            States state = (States)EditorGUILayout.EnumPopup("Animation State", (States)anim.controller.GetInteger(AudienceAnimHandlerBasic._paramIdState));
            anim.controller.SetInteger(AudienceAnimHandlerBasic._paramIdState, (int)state);

            //float blend0 = anim.controller.GetFloat(AudienceAnimHandlerBasic._paramIdBlendFactor0);
            //blend0 = EditorGUILayout.Slider("Blend Factor 0", blend0, 0f, 1f);
            //anim.controller.SetFloat(AudienceAnimHandlerBasic._paramIdBlendFactor0, blend0);

            //float blend1 = anim.controller.GetFloat(AudienceAnimHandlerBasic._paramIdBlendFactor1);
            //blend1 = EditorGUILayout.Slider("Blend Factor 1", blend1, -1f, 1f);
            //anim.controller.SetFloat(AudienceAnimHandlerBasic._paramIdBlendFactor0, blend1);

            if (lv == Level.Full)
            {
                int subState = anim.controller.GetInteger(AudienceAnimHandlerBasic._paramIdSubState);
                subState = EditorGUILayout.IntSlider("Sub State", subState, 0, AudienceAnimHandlerFull.subStateNum[state] - 1);
                anim.controller.SetInteger(AudienceAnimHandlerBasic._paramIdSubState, subState);
                bool mirror = anim.controller.GetBool(AudienceAnimHandlerBasic._paramIdMirror);
                mirror = EditorGUILayout.Toggle("Mirror", mirror);
                anim.controller.SetBool(AudienceAnimHandlerBasic._paramIdMirror, mirror);
            }
            
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Separator();
        }

        EditorGUILayout.LabelField("Static Area (No Serialization)", EditorStyles.boldLabel);

        GUI.enabled = false;
        AudienceAnimHandlerBasic.eyeIconOffset = EditorGUILayout.Vector3Field("Eye Icon Offset", AudienceAnimHandlerBasic.eyeIconOffset);
        AudienceAnimHandlerBasic.eyeIconScale = EditorGUILayout.FloatField("Eye Icon Scale", AudienceAnimHandlerBasic.eyeIconScale);
        AudienceAnimHandlerBasic.eyeIconFreq = EditorGUILayout.FloatField("Eye Icon Freq", AudienceAnimHandlerBasic.eyeIconFreq);
        GUI.enabled = true;

        if (lv != Level.Full)
        {
            if (AudienceAnimClipHolder.curr != null)
            {
                EditorGUILayout.LabelField("Clip Shuffle", EditorStyles.boldLabel);
                EditorHelper.ShowArray(ref AudienceAnimClipHolder.curr.focusedClips, "Focused Clips");
                EditorHelper.ShowArray(ref AudienceAnimClipHolder.curr.boredClips, "Bored Clips");
                EditorHelper.ShowArray(ref AudienceAnimClipHolder.curr.chattingClips, "Chatting Clips");
            }
            else
                EditorGUILayout.LabelField("No Basic Clips Holder Found!", EditorStyles.boldLabel);
        }
        serializedObject.ApplyModifiedProperties();
    }

}

#endif