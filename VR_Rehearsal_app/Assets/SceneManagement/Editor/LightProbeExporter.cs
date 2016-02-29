#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public static class LightProbeExporter
{
    [MenuItem("VR_Rehearsal_app/Light Probe Exporter/Export")]
    private static void Export()
    {
        string path = EditorUtility.SaveFilePanel("Export Path", Application.dataPath, "lightprobes", "bytes");
        LightProbes probes = LightmapSettings.lightProbes;
        if (!string.IsNullOrEmpty(path) && probes != null && probes.count > 0)
        {    
            using (var output = new BinaryWriter(File.OpenWrite(path)))
            {
                output.Write(probes.count);
                for (int i = 0; i < probes.count; ++i)
                {
                    for (int ch = 0; ch < 3; ++ch)
                        for (int coef = 0; coef < 9; ++coef)
                            output.Write(probes.bakedProbes[i][ch, coef]);
                }
                output.Close();
            }
        }
    }
}

#endif