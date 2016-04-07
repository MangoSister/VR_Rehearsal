using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System;

public class bShowcaseManager  {
	
	private const string _binaryFileName = "showcaseData";
	private const string _fileHeaderValidChecker = "@##@!)dfssjfdkjdkfjlsd@#!@";
	private int _numOfShowcase = 0;
	private Dictionary<string, showcase_Data> _showcaseTable;

	public struct showcase_Data{
		public string _showcaseID;
		public string _showcaseName;
		public ushort _mapIdx;
		public string _pptFolderPath;
		public ushort _percentageOfAudience;
		public ushort _expetedTime_min;

		public System.DateTime _updatedDate;
	
		public showcase_Data(string id, string name, ushort mapIdx, string pptPath, ushort percentage,ushort expectedTime){
			 _showcaseID = id; _showcaseName = name; _mapIdx = mapIdx; _pptFolderPath = pptPath;  _percentageOfAudience = percentage;
			_expetedTime_min = expectedTime;

			_updatedDate = System.DateTime.Now;
		}
	}

    public bShowcaseManager (){
        Start();
    }
		
	public bool Start(){
		if (_showcaseTable != null)
			_showcaseTable.Clear ();
		else
			_showcaseTable = new Dictionary<string, showcase_Data> ();

		#if UNITY_EDITOR
		string loadPath = Application.dataPath + "/" + _binaryFileName + ".bytes";
		Debug.Log (Application.persistentDataPath);
		#elif UNITY_ANDROID 
		string loadPath = Application.persistentDataPath + "/" + _binaryFileName + ".bytes";
		#endif

		if (!File.Exists (loadPath)) {
			SaveShowcasesBinaryInLocal ();
		}
			
		bool res = LoadShowcaseBinaryFromLocal ();
		return res;
	}

    /*
    public bool End(){
		bool res = SaveShowcasesBinaryInLocal ();
		_showcaseTable.Clear ();
		return res;
	}
	*/

	//Purpose for DateTime Sorting 
	private int DateTime_Compare(showcase_Data x, showcase_Data y){
		return System.DateTime.Compare (y._updatedDate, x._updatedDate);
	}

    public showcase_Data[] GetAllShowcases(){
		bool res = LoadShowcaseBinaryFromLocal ();
        if(!res) return null;

        showcase_Data[] arr = new showcase_Data[_showcaseTable.Count];
		int index = 0;
        foreach (KeyValuePair<string, showcase_Data> pair in _showcaseTable){
            arr[index] = (showcase_Data)pair.Value;
            index++;
        }
			
		Array.Sort (arr, DateTime_Compare);
        return arr;
    }

    public string AddShowcase(string caseName, int mapIdx, string pptFolderPath, int percentage, int expTime ){
        bool res = LoadShowcaseBinaryFromLocal();
        if (!res) return null;

        string tempId = System.DateTime.Now.ToString ("yyyy_MM_dd_hh_mm_ss");
		showcase_Data tempShowcase = new showcase_Data(tempId, caseName, (ushort)mapIdx, pptFolderPath, (ushort)percentage, (ushort)expTime);
		_showcaseTable.Add (tempId, tempShowcase);

		SaveShowcasesBinaryInLocal ();
		return tempId;
	}

	public bool EditShowcase_path(string caseID, string pptFolderPath){
		bool res = LoadShowcaseBinaryFromLocal();
		if (!res) return false;

		if (!_showcaseTable.ContainsKey(caseID))
			return false;
		
		showcase_Data tempShowcase = (showcase_Data)_showcaseTable [caseID];
		tempShowcase._pptFolderPath = pptFolderPath;
		tempShowcase._updatedDate = System.DateTime.Now;

		_showcaseTable [caseID] = tempShowcase;

		SaveShowcasesBinaryInLocal ();
		return true;

	}

	public bool EditShowcase(string caseID, string caseName, int mapIdx, string pptFolderPath, int percentage, int expTime ){

        bool res = LoadShowcaseBinaryFromLocal();
        if (!res) return false;

        if (!_showcaseTable.ContainsKey(caseID))
            return false;

        showcase_Data tempShowcase = (showcase_Data)_showcaseTable [caseID];
		tempShowcase._showcaseName = caseName;
		tempShowcase._mapIdx = (ushort)mapIdx;
		tempShowcase._pptFolderPath = pptFolderPath;
		tempShowcase._percentageOfAudience = (ushort)percentage;
		tempShowcase._expetedTime_min = (ushort)expTime;
		tempShowcase._updatedDate = System.DateTime.Now;

		_showcaseTable [caseID] = tempShowcase;

		SaveShowcasesBinaryInLocal ();
		return true;
	}

	public bool DeleteShowcase(string caseID){

        bool res = LoadShowcaseBinaryFromLocal();
        if (!res) return false;

        if (!_showcaseTable.ContainsKey(caseID))
            return false;

        try
        {
			string targetFolderPath = _showcaseTable[caseID]._pptFolderPath;
			if(Directory.Exists (targetFolderPath)){

				Directory.Delete(targetFolderPath,true);
			}

		}catch(IOException e){
			#if UNITY_EDITOR
			Debug.Log (e);
			#endif
		}

		_showcaseTable.Remove (caseID);

		SaveShowcasesBinaryInLocal ();
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
					if(_showcaseTable == null){
						_showcaseTable = new Dictionary<string, showcase_Data>();
					}else{
						_showcaseTable.Clear();
					}
						
					for (int i = 0; i < _numOfShowcase; ++i) {

						showcase_Data tempCase = new showcase_Data ();
						tempCase._showcaseID = r.ReadString ();
						tempCase._showcaseName = r.ReadString ();
						tempCase._mapIdx = (ushort)r.ReadInt16 ();
						tempCase._pptFolderPath = r.ReadString ();
						tempCase._percentageOfAudience = (ushort)r.ReadInt16 ();
						tempCase._expetedTime_min = (ushort)r.ReadInt16 ();
						//string -> DateTime
						string strToDateTime = r.ReadString ();
						tempCase._updatedDate = System.Convert.ToDateTime(strToDateTime);

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
					w.Write(((showcase_Data)pair.Value)._expetedTime_min);
					//DateTime -> string
					w.Write(((showcase_Data)pair.Value)._updatedDate.ToString("yyyy/MM/dd HH:mm:ss"));
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
