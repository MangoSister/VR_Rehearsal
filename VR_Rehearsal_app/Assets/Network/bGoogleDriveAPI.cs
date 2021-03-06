﻿using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bGoogleDriveAPI : MonoBehaviour {

    /*
	void Start(){
		_filesDictionary = new Dictionary<string, GoogleDrive.File> ();

		StartAuthentication (delegate() {
			Debug.Log ("Authentication is done");
			GetSelectedFolderFileList("", delegate(string filelists) {

				Debug.Log(filelists);

			});
		});

	}*/

	//Internal usage
	private GoogleDrive _drive;
	private bool _initInProgress = false;
	private bool _revokeInProgress = false;

	//For Function
	string parentFolderID = "";
	string recentFolderID = "none";
    string recentFolderName = "none";

	List<string> currPath;

	//For Callback
	//1. Authentication
	bool _isAuthenticationDone = false;
	bool _isAuthenticationSucceed = false;
	int  _AuthenticationResCode = 0;
	/*Authentication callback*/
	bhClowdDriveAPI.Authentication_Callback _authen_callback;

	//2. Filelist 
	bhClowdDriveAPI.fileList_Callback _updateList_callback;
	bool _isUpdateListProcessing = false;
	bool _isUpdateListDone = false;

	//3. FileDownload
	bhClowdDriveAPI.fileDownload_Callback _fileDownload_callback; 
	bhClowdDriveAPI.fileDownload_Process_Callback _fileDownload_proceed_callback;
	bhClowdDriveAPI.fileDownload_Cancel_Callback _fileDownload_cancel_callback;

	bool _isFileDownloadDone = false;
	bool _isSingleFileDownloadDone = false;
	bool _isFileDownloadProcessing = false;
	bool _isFileDownloadCancel = false;
	int _NumberOfTotalDownloadFile = 0;
	int _NumberOfProcessedFile = 0;


	//---------
	Dictionary<string, GoogleDrive.File> _filesDictionary ;

	// delegation
	delegate void funcResult();
	delegate void boolFuncResult(bool res, int resCode);

	//4. Revoke
	bhClowdDriveAPI.revoke_Callback _revoke_callback;
	bool isRevoked = false;


	void Update(){
		if (_isAuthenticationDone == true) {
			_authen_callback (_isAuthenticationSucceed, _AuthenticationResCode);
			_isAuthenticationDone = false;
		}

		if (_isUpdateListDone == true) {
			string json = GenerateBClowdFormatJson (_filesDictionary);
			_updateList_callback (json);
			_isUpdateListDone = false;
		}

		if (_isFileDownloadProcessing == true) {
			if (_isFileDownloadDone == true) {

				if (_isFileDownloadCancel) {
					_fileDownload_cancel_callback ();
					_isFileDownloadCancel = false;
				} else {
					_fileDownload_callback ();
				}

				_NumberOfTotalDownloadFile = 0;
				_NumberOfProcessedFile = 0;
				_isFileDownloadProcessing = false;
				_isFileDownloadDone = false;
			}
			if (_isSingleFileDownloadDone == true) {
				_fileDownload_proceed_callback (_NumberOfTotalDownloadFile, _NumberOfProcessedFile);
				_isSingleFileDownloadDone = false;
			}
		
		}

		if (isRevoked) {
			_revoke_callback ();
			isRevoked = false;
		}
			
	}
    private void Initialize() {
        if (_filesDictionary == null) {
            _filesDictionary = new Dictionary<string, GoogleDrive.File>();
        }

		if (currPath == null) {
			currPath = new List<string> ();
		} else {
			currPath.Clear ();
		}
	
    }

	public void StartAuthentication(bhClowdDriveAPI.Authentication_Callback callback){
        /*
		 * Autentciation checking
		 * If _initInProgress == false, this means drive need autentication
		 * and _initInProgress == true, this means drive doesn't need autentication process
		 */
        Initialize();
        _authen_callback = callback;

		if (_initInProgress == false) { 
			_isAuthenticationDone = false;
			StartCoroutine (StartAuthentication_internal (delegate(bool res, int resCode) {
				_isAuthenticationDone = true;
				_AuthenticationResCode = resCode;
				if(res)
					_isAuthenticationSucceed = true;
				else
					_isAuthenticationSucceed = false;

			},0));
		}
	}
		
	public void GetSelectedFolderFileList(string _selectedFolderName, bhClowdDriveAPI.fileList_Callback callback){
		if (_drive == null || _isUpdateListProcessing == true)
			return;

		_isUpdateListDone = false;
		_updateList_callback = callback;
		_isUpdateListProcessing = true;
		/*
        if (recentFolderID != "none") {
            parentFolderID = recentFolderID;
        }*/
		
		string id = "";
		if (_selectedFolderName != "") {
			/*
			string removedLastWhiteSpace = "";

			for (int i = 0; i < _selectedFolderName.Length - 1; i++) {
				removedLastWhiteSpace += _selectedFolderName [i];
			}*/

			//bool res = _filesDictionary.ContainsKey (_selectedFolderName);
			if (_filesDictionary != null &&_filesDictionary.ContainsKey(_selectedFolderName)) {
				id = _filesDictionary [_selectedFolderName].ID;
			}

			recentFolderName = _selectedFolderName;

			bool isDuplicated = false;
			foreach(string Path in currPath){
				if (Path == id) {
					isDuplicated = true;
				}
			}

			if (!isDuplicated) {
				currPath.Add (id);
			}


		}else{
			
			recentFolderID = id;
			recentFolderName = _selectedFolderName;
			//parentFolderID = "";
			currPath.Add ("");
		}


        StartCoroutine (GetFileLists_internal (id, delegate() {
			_isUpdateListDone = true;
			_isUpdateListProcessing = false;
		}));
	}

	public void  GetCurrParentFileList (bhClowdDriveAPI.fileList_Callback callback){
		if (_drive == null || _isUpdateListProcessing == true)
			return;

		_isUpdateListDone = false;
		_updateList_callback = callback; 
		_isUpdateListProcessing = true;

		int parentIdx = currPath.Count - 1;
		if (parentIdx < 0)
			parentIdx = 0;
		
		if (parentIdx != 0) {
			currPath.RemoveAt(currPath.Count - 1);
		}

		parentIdx -= 1;
		if (parentIdx < 0)
			parentIdx = 0;

		string bbbRes = currPath[parentIdx];
			
		StartCoroutine (GetFileLists_internal (bbbRes, delegate() {
			_isUpdateListDone = true;
			_isUpdateListProcessing = false;
		}));
	}

    public string GetRecentPath(){
		if (recentFolderID == "") 
			recentFolderName = "";
		else {
			foreach(KeyValuePair<string,GoogleDrive.File> file in _filesDictionary){
				if(file.Value.ID == recentFolderID){
					recentFolderName = file.Key;
				}
			}
		}

        return recentFolderName;
    }

	public  void CancelDownload (){
		if (_isFileDownloadProcessing != true)
			return;

		_isFileDownloadCancel = true;
	}

	public void FileDownloadAll(string loadFolderName, string saveFolderPath, bhClowdDriveAPI.fileDownload_Callback callback,  bhClowdDriveAPI.fileDownload_Process_Callback proceed_callback, bhClowdDriveAPI.fileDownload_Cancel_Callback cancel_callback ){

		if (_isFileDownloadProcessing == true)
			return;

		_fileDownload_callback = callback;
		_fileDownload_proceed_callback= proceed_callback;
		_fileDownload_cancel_callback = cancel_callback;

		StartCoroutine(DonwloadAllFilesInFolder_internal (loadFolderName, saveFolderPath) );
	} 

	public void Revoke(bhClowdDriveAPI.revoke_Callback callback){
		if (_revokeInProgress == true)
			return;

		_revoke_callback = callback;
		StartCoroutine (Revock_internal ());
	}

	IEnumerator Revock_internal(){
		_revokeInProgress = true;
		yield return StartCoroutine(_drive.Unauthorize());
		_revokeInProgress = false;
		isRevoked = true;
	}

	IEnumerator StartAuthentication_internal(boolFuncResult callback ,int trialNumber){
		_initInProgress = true;

		_drive = new GoogleDrive();
		_drive.ClientID = "251952116687-o29juik9i0qbl6ktpa0n97cavk8cvip4.apps.googleusercontent.com";
		_drive.ClientSecret = "0ZptNq9TwzL7enTtlRUOoZ3_";

		var authorization = _drive.Authorize();
		yield return StartCoroutine(authorization);

		if (authorization.Current is Exception) {
			
			#if UNITY_EDITOR
			#endif
			Exception temp =(authorization.Current as Exception);
			string res = temp.ToString();
		
			if(res == "GoogleDrive+Exception: Invalid credential."){
					callback (true, 1);
				
			}else{
				callback (false,0);
			}
				
		}else {
			#if UNITY_EDITOR
			Debug.Log("User Account: " + _drive.UserAccount);	
			#endif
			callback (true,0);
		}
			
		_initInProgress = false;
	}


	private string GenerateBClowdFormatJson(Dictionary<string, GoogleDrive.File> Dict){
		string jsonStr = "{\n   \"entries\":[\n   ";

		foreach(KeyValuePair<string, GoogleDrive.File> file in Dict){
			if (file.Value.MimeType == "application/vnd.google-apps.folder") {
                jsonStr += "{\n \".tag\":\"folder\",\n    \"name\":\"" + file.Key + "\",\n },\n ";
			} else {
				jsonStr += "{\n \".tag\":\"file\",\n  \"name\":\"" + file.Key + "\",\n  \"size\":\"" + file.Value.FileSize + "\",\n },\n ";
            }


		}
	

		jsonStr += " ],\n }";
		return jsonStr;
	}
		
	IEnumerator GetFileLists_internal(string Id, funcResult callback){
		var listFiles = _drive.ListFolders(Id);
		yield return StartCoroutine(listFiles);
		var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(listFiles);

		recentFolderID = Id;
		//parentFolderID = _filesDictionary [removedLastWhiteSpace].Parents [0];

		if (files != null){
			_filesDictionary.Clear ();
			foreach (GoogleDrive.File file in files){
				#if UNITY_EDITOR
				Debug.Log (file);
				#endif
				if (_filesDictionary.ContainsKey (file.Title)) {
					//Ignores Duplicated files 
					#if UNITY_EDITOR
					Debug.Log (file.Title);
					#endif
				} else {
					_filesDictionary.Add (file.Title, file);

					if (file.IsFolder == true) {
						parentFolderID = file.Parents [0];
					}
				}
			}
			callback ();
		}
		#if UNITY_EDITOR
		Debug.Log (files);
		#endif
	}


	IEnumerator DonwloadAllFilesInFolder_internal(string loadFolderName, string saveFolderPath){
		
		_isFileDownloadProcessing = true;

		_isFileDownloadDone = false;
		_isSingleFileDownloadDone = false;



		string targetId = "";
		if (loadFolderName != "") {
			if (_filesDictionary != null &&_filesDictionary.ContainsKey(loadFolderName)) {
				targetId = _filesDictionary [loadFolderName].ID;

			}
		}
			
		var listFiles = _drive.ListFolders(recentFolderID);

		yield return StartCoroutine(listFiles);
		var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(listFiles);


		if (files != null){
			
			//Processing Number Initialize !!
			if (!Directory.Exists (saveFolderPath)) {
				Directory.CreateDirectory(saveFolderPath);
			}


			//Processed file count
			_NumberOfTotalDownloadFile = 0;
			foreach (var file in files) {
				if (file.Title.EndsWith(".jpg") || file.Title.EndsWith(".png") || file.Title.EndsWith(".JPG") || file.Title.EndsWith(".PNG") ){
					_NumberOfTotalDownloadFile += 1;
				}
			}
			_NumberOfProcessedFile = 0;
			_isSingleFileDownloadDone = true;


			//Download Start;

			//fileName Duplicator Check
			List<string> fileNameList = new List<string>();

			foreach (var file in files){
				#if UNITY_EDITOR
				Debug.Log(file);
				#endif

				if (_isFileDownloadCancel)
					break;


				if (file.Title.EndsWith(".jpg") || file.Title.EndsWith(".png") || file.Title.EndsWith(".JPG") || file.Title.EndsWith(".PNG") ){
					var download = _drive.DownloadFile(file);
					yield return StartCoroutine(download);

					var data = GoogleDrive.GetResult<byte[]>(download);

					//Name Duplicator checker
					string finalFileTitle = file.Title;

					bool bIsDuplicatedFileName = false;
					int duplicatedNumber = 0;
					foreach (string title in fileNameList) {
						if (title == file.Title) {
							bIsDuplicatedFileName = true;
							duplicatedNumber++;
						}
					}

					if(bIsDuplicatedFileName)
					finalFileTitle += duplicatedNumber.ToString();

					try{
						FileStream fs = new FileStream (saveFolderPath + "/" + finalFileTitle, FileMode.Create);
						fs.Write(data, 0, data.Length);
						fs.Dispose ();

						fileNameList.Add (file.Title);

						//-----
						++_NumberOfProcessedFile;
						_isSingleFileDownloadDone = true;
					}catch{
						_isFileDownloadCancel = true;
					}



				}
			}

		}else{
			#if UNITY_EDITOR
			Debug.Log(listFiles.Current);
			#endif
		}

		_isFileDownloadDone = true;

	}




}
