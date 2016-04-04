using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(spaceInfoExpoter))]
public class spaceInfoExport_Inspector : Editor {

	public override void OnInspectorGUI() {
		//base.OnInspectorGUI();
		DrawDefaultInspector();

		if(GUILayout.Button("DisposeChair")) {
			spaceInfoExpoter exporter = (spaceInfoExpoter)target;
			exporter.Dispose();
			//Erasing function;;
		}

		if(GUILayout.Button("DeleteChairs")) {
			spaceInfoExpoter exporter = (spaceInfoExpoter)target;
			exporter.DeleteAll();
			//Erasing function;;
		}

		if(GUILayout.Button("Reset")) {
			spaceInfoExpoter exporter = (spaceInfoExpoter)target;
			exporter.Reset();
			//Erasing function;;
			

		}

		if(GUILayout.Button("Generate")) {
			spaceInfoExpoter exporter = (spaceInfoExpoter)target;
			exporter.Export();
			//Erasing function;;
			
			
		}
		
	
	}
}
