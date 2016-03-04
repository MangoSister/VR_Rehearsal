using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public class bShowcaseManager  {
	
	private const string _binaryFileName = "showcaseData";
	private const string _fileHeaderValidChecker = "@##@!)dfssjfdkjdkfjlsd@#!@";
	private int _numOfShowcase = 0;
	private Dictionary<string, showcase_Data> _showcaseTable;

	private struct showcase_Data{
		public string _showcaseID;
		public string _showcaseName;
		public ushort _mapIdx;
		public string _pptFolderPath;
		public ushort _percentageOfAudience;

		public showcase_Data(string id, string name, ushort mapIdx, string pptPath, ushort percentage){
			 _showcaseID = id; _showcaseName = name; _mapIdx = mapIdx; _pptFolderPath = pptPath;  _percentageOfAudience = percentage;
		}
	}

	public bool Start(){
		if (_showcaseTable != null)
			_showcaseTable.Clear ();

		_showcaseTable = new Dictionary<string, showcase_Data> ();
		bool res = LoadShowcaseBinaryFromLocal ();
		return res;
	}

	public bool End(){
		bool res = SaveShowcasesBinaryInLocal ();
		_showcaseTable.Clear ();
		return res;
	}
		
	public string AddShowcase(string caseName, int mapIdx, string pptFolderPath, int percentage ){

		string tempId = System.DateTime.Now.ToString ("yyyy_MM_dd_hh_mm_ss");
		showcase_Data tempShowcase = new showcase_Data(tempId, caseName, (ushort)mapIdx, pptFolderPath, (ushort)percentage);
		_showcaseTable.Add (tempId, tempShowcase);
		return tempId;
	}

	public bool EditShowcase(string caseID, string caseName, int mapIdx, string pptFolderPath, int percentage ){

		if (!_showcaseTable.ContainsKey(caseID))
			return false;

		showcase_Data tempShowcase = (showcase_Data)_showcaseTable [caseID];
		tempShowcase._showcaseName = caseName;
		tempShowcase._mapIdx = (ushort)mapIdx;
		tempShowcase._pptFolderPath = pptFolderPath;
		tempShowcase._percentageOfAudience = (ushort)percentage;

		_showcaseTable [caseID] = tempShowcase;
		return true;
	}

	public bool DeleteShowcase(string caseID){
		if (!_showcaseTable.ContainsKey(caseID))
			return false;

		_showcaseTable.Remove (caseID);
		return true;
	}
		
	private bool LoadShowcaseBinaryFromLocal(){
		#if UNITY_EDITOR
		string loadPath = Application.dataPath + "/" + _binaryFileName + ".bytes";
		#elif UNITY_ANDROID 
		string loadPath = Application.persistentDataPath + "/" + _binaryFileName + ".bytes";
		#endif

		if (File.Exists (loadPath)) {
			try{
				using (BinaryReader r = new BinaryReader (File.Open (loadPath, FileMode.Open))) {
					//1. Header checking in order to check file corruption
					string tempHeader = r.ReadString ();
					if (tempHeader != _fileHeaderValidChecker)
						return false;
					//2. numOfshowcase
					_numOfShowcase = r.ReadInt32 ();

					//3. Load Showcases
					for (int i = 0; i < _numOfShowcase; ++i) {
						
						showcase_Data tempCase = new showcase_Data ();
						tempCase._showcaseID = r.ReadString ();
						tempCase._showcaseName = r.ReadString ();
						tempCase._mapIdx = (ushort)r.ReadInt16 ();
						tempCase._pptFolderPath = r.ReadString ();
						tempCase._percentageOfAudience = (ushort)r.ReadInt16 ();

						_showcaseTable.Add (tempCase._showcaseID, tempCase);
					}

					//4. End
					string tempfooter = r.ReadString ();
					if (tempHeader != _fileHeaderValidChecker) {
						_showcaseTable.Clear ();
						return false;
					} else {
						return true;
					}
				}
			}catch{
				return false;
			}

		} else {
			return false;
		}
	}

	private bool SaveShowcasesBinaryInLocal(){
		#if UNITY_EDITOR
		string savePath = Application.dataPath + "/" + _binaryFileName + ".bytes";
		#elif UNITY_ANDROID 
		string savePath = Application.persistentDataPath + "/" + _binaryFileName + ".bytes";
		#endif

		try{
			using(var w = new BinaryWriter(File.OpenWrite(savePath))){
				//1. Header checking in order to check file corruption
				w.Write(_fileHeaderValidChecker);
				//2. Number Of Showcase
				w.Write(_showcaseTable.Count);

				//3.save each showcase 
				foreach (KeyValuePair<string, showcase_Data> pair in _showcaseTable){
					
					w.Write(((showcase_Data)pair.Value)._showcaseID);
					w.Write(((showcase_Data)pair.Value)._showcaseName);
					w.Write(((showcase_Data)pair.Value)._mapIdx);
					w.Write(((showcase_Data)pair.Value)._pptFolderPath);
					w.Write(((showcase_Data)pair.Value)._percentageOfAudience);
				}
					
				//4. End
				w.Write(_fileHeaderValidChecker);
				w.Close();
			}
		}catch{
			return false;
		}

		return true;
	}




}
