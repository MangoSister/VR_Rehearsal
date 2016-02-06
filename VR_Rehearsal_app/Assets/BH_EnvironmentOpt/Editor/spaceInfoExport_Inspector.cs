using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(spaceInfoExpoter))]
public class spaceInfoExport_Inspector : Editor {

	public override void OnInspectorGUI() {
		//base.OnInspectorGUI();
		DrawDefaultInspector();
		
		if(GUILayout.Button("Export!!")) {
			spaceInfoExpoter exporter = (spaceInfoExpoter)target;
			
			//Erasing function;
			exporter.Export();

		}

		if(GUILayout.Button("Refresh")) {
			spaceInfoExpoter exporter = (spaceInfoExpoter)target;
			exporter.Reset();
		}

		
	}
}
