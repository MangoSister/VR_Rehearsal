#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using SoundType = AudioManager.SoundType;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerInspector : Editor
{
    private AudioManager am { get { return target as AudioManager; } }
    private SoundType newType;
    private AudioClip newClip;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        SerializableDictionary<SoundType, AudioClip> copy;
        if (am.clipDict != null)
            copy = new SerializableDictionary<SoundType, AudioClip>(am.clipDict);
        else
            copy = new SerializableDictionary<SoundType, AudioClip>();

        EditorHelper.ShowEnumDict(ref copy, ref newType, ref newClip, "Environments");
        am.clipDict = new AudioManager.SoundDict(copy);

        serializedObject.ApplyModifiedProperties();
    }
}

#endif