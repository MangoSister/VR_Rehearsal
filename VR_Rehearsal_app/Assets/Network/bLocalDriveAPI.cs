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

		if (!Directory.Exists (_recentPath))
			return false;

		string jsonStr = "{\n   \"entries\":[\n   ";
		string[] files = Directory.GetFiles (path);
		string[] folders = Directory.GetDirectories (path);

		foreach (string file in files) {
			string[] stringSeparators = new string[] {"/", "\\"};
			string[] result = file.Split(stringSeparators, StringSplitOptions.None);
			jsonStr += "{\n \".tag\":\"file\",\n    \"name\":\""+  result[result.Length - 1] + "\",\n },\n ";
		}
		foreach (string folder in folders) {
			string[] stringSeparators = new string[] {"\\", "/"};
			string[] result = folder.Split(stringSeparators, StringSplitOptions.None);
			jsonStr += "{\n \".tag\":\"folder\",\n    \"name\":\""+  result[result.Length - 1] + "\",\n },\n ";
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
		for (int i = 0; i < (result.Length -2 ); ++i) {

            if (i == 0)
            {
                _recentPath = result[0];
            }
            else {
                _recentPath += "/" + result[i];
            } 

		}

		if (_recentPath == "") {
			_recentPath = "/";
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

		_recentPath += "/";

		GetFileListFromPath (_recentPath, callback);
		return true;
	}

	public override bool DownloadFile (string filename, string savePath,string saveName, fileDownload_Callback callback){

		File.Copy( (_recentPath + "/" + filename ), (savePath + "/" + saveName ));
		callback ();
		return true;
	}

	public override bool DonwloadAllFilesInFolder(string loadFolderPath, string saveFolderPath,fileDownload_Callback callback,  fileDownload_Process_Callback proceed_callback, fileDownload_Cancel_Callback cancel_callback){

		GetFileListFromPath (loadFolderPath, delegate(string resJson) {

			if (!File.Exists (saveFolderPath)) {
				Directory.CreateDirectory(saveFolderPath);
			}	

			JSONNode parseResult = JSON.Parse (resJson);
			for (int index = 0; index < parseResult ["entries"].Count; index++) {
				if (parseResult ["entries"] [index] [".tag"].Value == "file") {
					DownloadFile (parseResult ["entries"] [index] ["name"].Value, saveFolderPath, parseResult ["entries"] [index] ["name"].Value, delegate() {
						proceed_callback(parseResult ["entries"].Count, index);
					});
				}
			}

			callback ();


		});
		return true;
	}

	public override void CancelDownload (){

	}

	public override void JobDone (){
	}

	public override string GetAPItoken (){
		return "None: bLocalDrive Does not need API Token";
	}
	public override string GetRecentPath (){
		return _recentPath;
	}
	public override void StartAuthentication (Authentication_Callback callback){callback (true);}
	public override void Update (){} 
	public override void Revoke(){}
}
