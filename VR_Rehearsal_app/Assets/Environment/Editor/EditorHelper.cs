#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public static class EditorHelper
{
    private static GUIContent
        deleteButtonContent = new GUIContent("-", "delete"),
        addButtonContent = new GUIContent("+", "add");
    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

    public static void ShowArray<T>(ref T[] array, string title) where T : Object
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        int num = EditorGUILayout.IntField("Length", array == null ? 0 : array.Length);
        if (num > 0 && array == null)
            array = new T[num];
        else if (num == 0 && array == null)
            return;
        else if (array != null && num != array.Length)
            array = new T[num];

        for (int i = 0; i < num; i++)
        {
            array[i] = EditorGUILayout.ObjectField(string.Format("Element {0}", i), array[i], typeof(T), false) as T;
        }
    }

    public static void ShowEnumDict<TKey, TValue>(ref SerializableDictionary<TKey, TValue> dict, ref TKey newKey, ref TValue newObj, string title)
        where TKey : struct
        where TValue : Object
    {
        if (!typeof(TKey).IsEnum)
            return;

        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        if (dict != null)
        {
            List<TKey> keyToRemove = new List<TKey>();
            List<KeyValuePair<TKey, TValue>> entryToChange = new List<KeyValuePair<TKey, TValue>>();
            foreach (var entry in dict)
            {
                Rect rect = EditorGUILayout.BeginHorizontal(GUI.skin.box);
                GUI.enabled = false;
                EditorGUILayout.EnumPopup((entry.Key) as System.Enum);
                GUI.enabled = true;
                TValue val = EditorGUILayout.ObjectField(entry.Value, typeof(TValue), true) as TValue;
                if (val != entry.Value)
                    entryToChange.Add(new KeyValuePair<TKey, TValue>(entry.Key, val));
                if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
                    keyToRemove.Add(entry.Key);
                EditorGUILayout.EndHorizontal();
            }

            foreach (var changeEntry in entryToChange)
                dict[changeEntry.Key] = changeEntry.Value;

            foreach (var key in keyToRemove)
                dict.Remove(key);
        }
        EditorGUILayout.LabelField("New Dictionary Entry", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal(GUI.skin.box);
        System.Enum newKeyEnum = EditorGUILayout.EnumPopup(newKey as System.Enum);
        newKey = (TKey)System.Enum.Parse(typeof(TKey), newKeyEnum.ToString());
        newObj = EditorGUILayout.ObjectField(newObj, typeof(TValue), true) as TValue;
        if (GUILayout.Button(addButtonContent, EditorStyles.miniButtonRight, miniButtonWidth))
        {
            if (dict == null)
            {
                dict = new SerializableDictionary<TKey, TValue>() { { newKey, newObj } };
            }
            else if (!dict.ContainsKey(newKey))
                dict.Add(newKey, newObj);
        }
        EditorGUILayout.EndHorizontal();
    }
}

#endif