#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

public class VertexLitOrderedInspector : ShaderGUI
{
    private static readonly int[] options = 
        {-9, -8, -7, -6, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private static readonly string[] optionNames = 
        {"-9", "-8", "-7", "-6", "-5", "-4", "-3", "-2", "-1", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
    private const int GeometryQueueNum = 2000;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        base.OnGUI(materialEditor, properties);
        Material mat = materialEditor.target as Material;
        string[] keywords = mat.shaderKeywords;
        // mat.renderQueue

        int order = mat.renderQueue - GeometryQueueNum;
        EditorGUI.BeginChangeCheck();
        order = EditorGUILayout.IntPopup("Render Order (Relative to Geometry)", order, optionNames, options);
        mat.renderQueue = GeometryQueueNum + order;
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(mat);
        }
    }
}

#endif