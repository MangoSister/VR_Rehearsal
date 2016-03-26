using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class bGoogleDriveAPI : MonoBehaviour {


	void Start(){
		_filesDictionary = new Dictionary<string, GoogleDrive.File> ();

		StartAuthentication (delegate() {
			Debug.Log ("Authentication is done");
			GetSelectedFolderFileList("", delegate(string filelists) {

				Debug.Log(filelists);

			});
		});

	}

	//Internal usage
	private GoogleDrive _drive;
	private bool _initInProgress = false;
	private bool _revokeInProgress = false;

	//For Function
	string parentFolderID = "none";
	string recentFolderID = "none";

	//For Callback
	//1. Authentication
	bool _isAuthenticationDone = false;
	/*Authentication callback*/
	bhClowdDriveAPI.Authentication_Callback _authen_callback;

	//2. Filelist 
	bhClowdDriveAPI.fileList_Callback _updateList_callback;
	bool _isUpdateListDone = false;

	//3. FileDownload
	bhClowdDriveAPI.fileDownload_Callback _fileDownload_callback; 
	bhClowdDriveAPI.fileDownload_Process_Callback _fileDownload_proceed_callback;
	bool _isFileDownloadDone = false;
	bool _isSingleFileDownloadDone = false;
	bool _isFileDownloadProcessing = false;
	int _NumberOfTotalDownloadFile = 0;
	int _NumberOfProcessedFile = 0;


	//---------
	Dictionary<string, GoogleDrive.File> _filesDictionary ;

	// delegation
	delegate void funcResult();
	delegate void boolFuncResult(bool res);

	void Update(){
		if (_isAuthenticationDone == true) {
			_authen_callback ();
			_isAuthenticationDone = false;
		}

		if (_isUpdateListDone == true) {
			string json = GenerateBClowdFormatJson (_filesDictionary);
			_updateList_callback (json);
			_isUpdateListDone = false;
		}

		if (_isFileDownloadProcessing == true) {
			if (_isFileDownloadDone == true) {
				_fileDownload_callback ();

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
			
	}

	public void StartAuthentication(bhClowdDriveAPI.Authentication_Callback callback){
		/*
		 * Autentciation checking
		 * If _initInProgress == false, this means drive need autentication
		 * and _initInProgress == true, this means drive doesn't need autentication process
		 */
		_authen_callback = callback;

		if (_initInProgress == false) { 
			_isAuthenticationDone = false;
			StartCoroutine (StartAuthentication_internal (delegate(bool res) {
				if(res)
					_isAuthenticationDone = true;
				else
					_isAuthenticationDone = false;
			}));
		}
	}
		
	public void GetSelectedFolderFileList(string _selectedFolderName, bhClowdDriveAPI.fileList_Callback callback){
		if (_drive == null)
			return;

		_isUpdateListDone = false;
		_updateList_callback = callback; 

		parentFolderID = recentFolderID;

		string id = "";
		if (_selectedFolderName != "") {
			if (_filesDictionary != null &&_filesDictionary.ContainsKey(_selectedFolderName)) {
				id = _filesDictionary [_selectedFolderName].ID;
			}
		}
		recentFolderID = id;

		StartCoroutine (GetFileLists_internal (id, delegate() {
			_isUpdateListDone = true;
		}));
	}

	public void  GetCurrParentFileList (bhClowdDriveAPI.fileList_Callback callback){
		if (_drive == null)
			return;

		_isUpdateListDone = false;
		_updateList_callback = callback; 

		string id = "";
		if (parentFolderID != "none") {
			id = parentFolderID;
		}

		StartCoroutine (GetFileLists_internal (id, delegate() {
			_isUpdateListDone = true;
		}));
	}

	public void FileDownloadAll(string loadFolderName, string saveFolderPath, bhClowdDriveAPI.fileDownload_Callback callback,  bhClowdDriveAPI.fileDownload_Process_Callback proceed_callback ){

		if (_isFileDownloadProcessing == true)
			return;

		_fileDownload_callback = callback;
		_fileDownload_proceed_callback= proceed_callback;

		DonwloadAllFilesInFolder_internal (loadFolderName, saveFolderPath);
	} 

		
	IEnumerator StartAuthentication_internal(boolFuncResult callback){
		_initInProgress = true;

		_drive = new GoogleDrive();
		_drive.ClientID = "251952116687-bl6cbb0n9veq5ovirpk5n99pjlgtf16g.apps.googleusercontent.com";
		_drive.ClientSecret = "z65O11Za6aB74a7r21_TbtFL";

		var authorization = _drive.Authorize();
		yield return StartCoroutine(authorization);

		if (authorization.Current is Exception) {
			#if UNITY_EDITOR
			Debug.LogWarning (authorization.Current as Exception);
			goto finish;
			#endif
		} else {
			#if UNITY_EDITOR
			Debug.Log("User Account: " + _drive.UserAccount);	
			#endif
			callback (_initInProgress);
		}

		finish:
		_initInProgress = false;
	}

	IEnumerator Revock_internal(){
		_revokeInProgress = true;
		yield return StartCoroutine(_drive.Unauthorize());
		_revokeInProgress = false;
	}

	private string GenerateBClowdFormatJson(Dictionary<string, GoogleDrive.File> Dict){
		string jsonStr = "{\n   \"entries\":[\n   ";

		foreach(KeyValuePair<string, GoogleDrive.File> file in Dict){
			if (file.Value.MimeType == "application/vnd.google-apps.folder") {
				jsonStr += "{\n \".tag\":\"file\",\n    \"name\":\"" + file.Key + " \",\n },\n ";
			} else {
				jsonStr += "{\n \".tag\":\"folder\",\n    \"name\":\""+  file.Key + " \",\n },\n ";
			}
		}

		jsonStr += " ],\n }";
		return jsonStr;
	}
		
	IEnumerator GetFileLists_internal(string Id, funcResult callback){
		var listFiles = _drive.ListFolders(Id);
		yield return StartCoroutine(listFiles);
		var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(listFiles);

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
			
		var listFiles = _drive.ListFolders(targetId);

		yield return StartCoroutine(listFiles);
		var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(listFiles);

		if (files != null){
			
			//Processing Number Initialize !!
			_NumberOfTotalDownloadFile = files.Count;
			_NumberOfProcessedFile = 0;
			_isSingleFileDownloadDone = true;

			foreach (var file in files){
				#if UNITY_EDITOR
				Debug.Log(file);
				#endif
				if (file.Title.EndsWith(".jpg") || file.Title.EndsWith(".png") || file.Title.EndsWith(".JPG") || file.Title.EndsWith(".PNG") ){
					var download = _drive.DownloadFile(file);
					yield return StartCoroutine(download);

					var data = GoogleDrive.GetResult<byte[]>(download);

					try{
						FileStream fs = new FileStream (saveFolderPath + "/" + file.Title, FileMode.Create);
						fs.Write(data, 0, data.Length);
					}catch(IOException e){
						#if UNITY_EDITOR
						Debug.LogAssertion (e);
						#endif
						break;
					}
					++_NumberOfProcessedFile;
					_isSingleFileDownloadDone = true;
				}
			}
		}else{
			#if UNITY_EDITOR
			Debug.LogError(listFiles.Current);
			#endif
		}

		_isSingleFileDownloadDone = true;
	}




}
