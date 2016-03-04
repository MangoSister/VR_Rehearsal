﻿#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SimModule = CrowdSimulator.SimModule;

[CustomEditor(typeof(CrowdSimulator))]
public class CrowdSimulatorInspector : Editor
{
    private GUIStyle _gazeBlockStyle;
    private GUIStyle gazeBlockStyle
    {
        get
        {
            if (_gazeBlockStyle == null)
            {
                _gazeBlockStyle = new GUIStyle();
                _gazeBlockStyle.normal.background = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                Color[] colors = new Color[_gazeBlockStyle.normal.background.width *
                            _gazeBlockStyle.normal.background.height];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i].r = 0.2f;
                    colors[i].a = 1.0f;
                }
                _gazeBlockStyle.normal.background.SetPixels(colors);
                _gazeBlockStyle.normal.background.Apply();
            }
            

            return _gazeBlockStyle;
        }
    }

    private GUIStyle _voiceVolumeBlockStyle;
    private GUIStyle voiceBlockStyle
    {
        get
        {
            if (_voiceVolumeBlockStyle == null)
            {
                _voiceVolumeBlockStyle = new GUIStyle();
                _voiceVolumeBlockStyle.normal.background = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                Color[] colors = new Color[_voiceVolumeBlockStyle.normal.background.width *
                                            _voiceVolumeBlockStyle.normal.background.height];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i].g = 0.2f;
                    colors[i].a = 1.0f;
                }
                _voiceVolumeBlockStyle.normal.background.SetPixels(colors);
                _voiceVolumeBlockStyle.normal.background.Apply();
            }
            
            return _voiceVolumeBlockStyle;
        }
    }

    private GUIStyle _fillerwordsBlockStyle;
    private GUIStyle fillerwordsBlockStyle
    {
        get
        {
            if (_fillerwordsBlockStyle == null)
            {
                _fillerwordsBlockStyle = new GUIStyle();
                _fillerwordsBlockStyle.normal.background = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                Color[] colors = new Color[_fillerwordsBlockStyle.normal.background.width *
                                            _fillerwordsBlockStyle.normal.background.height];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i].b = 0.2f;
                    colors[i].a = 1.0f;
                }
                _fillerwordsBlockStyle.normal.background.SetPixels(colors);
                _fillerwordsBlockStyle.normal.background.Apply();
            }
            
            return _fillerwordsBlockStyle;
        }
    }

    private GUIStyle _seatBlockStyle;
    private GUIStyle seatBlockStyle
    {
        get
        {
            if (_seatBlockStyle == null)
            {
                _seatBlockStyle = new GUIStyle();
                _seatBlockStyle.normal.background = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                Color[] colors = new Color[_seatBlockStyle.normal.background.width *
                                            _seatBlockStyle.normal.background.height];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i].r = 0.2f;
                    colors[i].g = 0.2f;
                    colors[i].a = 1.0f;
                }
                _seatBlockStyle.normal.background.SetPixels(colors);
                _seatBlockStyle.normal.background.Apply();
            }
            
            return _seatBlockStyle;
        }
    }

    private GUIStyle _socialBlockStyle;
    private GUIStyle socialBlockStyle
    {
        get
        {
            if (_socialBlockStyle == null)
            {
                _socialBlockStyle = new GUIStyle();
                _socialBlockStyle.normal.background = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                Color[] colors = new Color[_socialBlockStyle.normal.background.width *
                                            _socialBlockStyle.normal.background.height];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i].g = 0.2f;
                    colors[i].b = 0.2f;
                    colors[i].a = 1.0f;
                }
                _socialBlockStyle.normal.background.SetPixels(colors);
                _socialBlockStyle.normal.background.Apply();
            }

            return _socialBlockStyle;
        }
    }

    private GUIStyle _globalBlockStyle;
    private GUIStyle globalBlockStyle
    {
        get
        {
            if (_globalBlockStyle == null)
            {
                _globalBlockStyle = new GUIStyle();
                _globalBlockStyle.normal.background = new Texture2D(2, 2, TextureFormat.ARGB32, false, true);
                Color[] colors = new Color[_globalBlockStyle.normal.background.width *
                                            _globalBlockStyle.normal.background.height];
                for (int i = 0; i < colors.Length; ++i)
                {
                    colors[i].r = 0.2f;
                    colors[i].b = 0.2f;
                    colors[i].a = 1.0f;
                }
                _globalBlockStyle.normal.background.SetPixels(colors);
                _globalBlockStyle.normal.background.Apply();
            }
            
            return _globalBlockStyle;
        }
    }

    private Dictionary<SimModule, bool> groupSwitch = new Dictionary<SimModule, bool>
    {
        {SimModule.Gaze, true },
        {SimModule.VoiceVolume, true },
        {SimModule.FillerWords, true },
        {SimModule.SeatDistribution, true },
        {SimModule.SocialGroup, true },
        {SimModule.Global, true }
    };

    private CrowdSimulator sim { get { return target as CrowdSimulator; } }
    public override void OnInspectorGUI()
    {
       // base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.ObjectField("Script:", MonoScript.FromMonoBehaviour(sim), typeof(MonoScript), false);

        EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabsL0"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabsL1"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabsL2"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabsL3"), true);
        sim.crowdParent = EditorGUILayout.ObjectField("Crowd Parent", sim.crowdParent, typeof(Transform), true) as Transform;
        sim.crowdConfigFileName = EditorGUILayout.TextField("Crowd Config File Name", sim.crowdConfigFileName);
        sim.stepInterval = EditorGUILayout.FloatField("Step Interval", sim.stepInterval);
        sim.stepExternalInterval = EditorGUILayout.FloatField("External Step Interval", sim.stepExternalInterval);
        sim.deterministic = EditorGUILayout.Toggle("Deterministic", sim.deterministic);
        EditorGUILayout.Separator();

        var keys = new List<SimModule>(groupSwitch.Keys);
        foreach (var key in keys)
        {
            if ((sim.simModule & key) != 0x00)
                groupSwitch[key] = true;
            else groupSwitch[key] = false;
        }


        
        var rect = EditorGUILayout.BeginVertical(gazeBlockStyle);
        groupSwitch[SimModule.Gaze] = EditorGUILayout.BeginToggleGroup(SimModule.Gaze.ToString(), groupSwitch[SimModule.Gaze]);
        sim.gazeCollision = EditorGUILayout.ObjectField("Gaze Collision", sim.gazeCollision, typeof(SimpleGazeCollision), true) as SimpleGazeCollision;
        sim.gazeCumulativeIntensity = EditorGUILayout.Slider("Gaze Cumulative Intensity", sim.gazeCumulativeIntensity, 0f, 1f);
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(voiceBlockStyle);
        groupSwitch[SimModule.VoiceVolume] = EditorGUILayout.BeginToggleGroup(SimModule.VoiceVolume.ToString(), groupSwitch[SimModule.VoiceVolume]);

        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(fillerwordsBlockStyle);
        groupSwitch[SimModule.FillerWords] = EditorGUILayout.BeginToggleGroup(SimModule.FillerWords.ToString(), groupSwitch[SimModule.FillerWords]);

        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(seatBlockStyle);
        groupSwitch[SimModule.SeatDistribution] = EditorGUILayout.BeginToggleGroup(SimModule.SeatDistribution.ToString(), groupSwitch[SimModule.SeatDistribution]);
        sim.seatPosAttentionFactor = EditorGUILayout.CurveField("Seat Attention Distribution", sim.seatPosAttentionFactor);
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(socialBlockStyle);
        groupSwitch[SimModule.SocialGroup] = EditorGUILayout.BeginToggleGroup(SimModule.SocialGroup.ToString(), groupSwitch[SimModule.SocialGroup]);
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(globalBlockStyle);
        groupSwitch[SimModule.Global] = EditorGUILayout.BeginToggleGroup(SimModule.Global.ToString(), groupSwitch[SimModule.Global]);
        sim.globalAttentionMean = EditorGUILayout.FloatField("Mean (Gaussian)", sim.globalAttentionMean);
        sim.globalAttentionStDev = EditorGUILayout.FloatField("Std Deviation (Gaussian)", sim.globalAttentionStDev);
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        sim.simModule = 0x00;
        foreach (var pair in groupSwitch)
        {
            if (pair.Value)
                sim.simModule |= pair.Key;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif