using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;


public class bLocalDriveAPI : bhClowdDriveAPI {
	private string _recentPath;

	public override bool GetFileListFromPath (string path, fileList_Callback callback){
		_recentPath = path;

		string jsonStr = "{\n   \"entries\":[\n   ";
		string[] files = Directory.GetFiles (path);
		string[] folders = Directory.GetDirectories (path);

		foreach (string file in files) {
			string[] stringSeparators = new string[] {"/", "\\"};
			string[] result = file.Split(stringSeparators, StringSplitOptions.None);
			jsonStr += "{\n \".tag\":\"file\",\n    \"name\":\""+  result[result.Length - 1] + " \",\n },\n ";
		}
		foreach (string folder in folders) {
			string[] stringSeparators = new string[] {"\\", "/"};
			string[] result = folder.Split(stringSeparators, StringSplitOptions.None);
			jsonStr += "{\n \".tag\":\"folder\",\n    \"name\":\""+  result[result.Length - 1] + " \",\n },\n ";
		}

		jsonStr += " ],\n }";

		callback(jsonStr);
		return true;
	}
		
	public override bool GetCurrParentFileList (fileList_Callback callback){
		/* "/" is root folder */
		if (_recentPath == "/") {
			return false;
		}

		string[] stringSeparators = new string[] {"/", "\\"};
		string[] result = _recentPath.Split(stringSeparators, StringSplitOptions.None);
		//example
		//recentFolder = "aaa/bbb/ccc"
		//cut it = "aaa/bbb/"
		for (int i = 0; i < (result.Length -1 ); ++i) {
			if (i == 0) {
				_recentPath = result [0];
			} 
			_recentPath += "/" + result [i];
		}

		GetFileListFromPath (_recentPath, callback);
		return true;
	}


	public override bool GetSelectedFolderFileList (string _selectedFolderName, fileList_Callback callback){
		if (_recentPath == "/") {
			_recentPath += _selectedFolderName;
		} else {
			_recentPath += "/" + _selectedFolderName;
		}

		GetFileListFromPath (_recentPath, callback);
		return true;
	}

	public override bool DownloadFile (string filename, string savePath,string saveName, fileDownload_Callback callback){


		File.Move( (_recentPath + "/" + filename ), (savePath + "/" + saveName ));
		callback ();
		return true;
	}

	public override bool DonwloadAllFilesInFolder(string loadFolderPath, string saveFolderPath,fileDownload_Callback callback){

		GetFileListFromPath (loadFolderPath, delegate(string resJson) {

			if (!File.Exists (saveFolderPath)) {
				Directory.CreateDirectory(saveFolderPath);
			}	

			var parseResult = JSON.Parse (resJson);
			for (int index = 0; index < parseResult ["entries"].Count; index++) {
				if (parseResult ["entries"] [index] [".tag"].Value == "file") {
					DownloadFile (parseResult ["entries"] [index] ["name"].Value, saveFolderPath, parseResult ["entries"] [index] ["name"].Value, delegate() {
						
					});
				}
			}
			callback ();
		});
		return true;
	}

	public override void JobDone (){
	}

	public override string GetAPItoken (){
		return "None: bLocalDrive Does not need API Token";
	}
	public override string GetRecentPath (){
		return _recentPath;
	}
	public override void StartAuthentication (){}
	public override void Update (){} 
}
