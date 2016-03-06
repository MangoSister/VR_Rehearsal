using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using SimpleJSON;
using System.Threading;
using System.ComponentModel;


public class bDropboxAPI : bhClowdDriveAPI{

	private string _token;
	private int _timeOut;
	private JobStatus _status = JobStatus.NotStarted;
	private bool _isGetToken = false;

	/*Authentication callback*/
	Authentication_Callback _authen_callback;

	/* Get List update variable */
	BackgroundWorker _updateList_bw; 
	fileList_Callback _updateList_callback;
	bool _isUpdateListDone = false;
	string _updateList_result;
	string _recentPath;
	private struct updateList_argData{
		public string _token;
		public string _path;
		public updateList_argData(string token, string path){_token = token; _path = path;}
	}
	/*Download single File*/
	BackgroundWorker _downloadFile_bw;
	fileDownload_Callback _downloadFile_callback;
	bool _isDownloadFileDone = false;
	string _downloadFil_result;
	private struct downloadFile_argData {
		public string _token;
		public string _filePath;
		public string _savePath;
		public string _saveName;
		public downloadFile_argData(string token, string filePath,string savePath, string saveName){_token = token; _filePath =  filePath;  _savePath = savePath; _saveName =saveName; }
	}
	/*Download Multiple Files */
	string _recentSaveFolderPath;
	bool _isDownloadMultipleFilesDone = false;
	fileDownload_Callback _downloadMultipleFile_callback;
	int processIdx = 0;

	public override void StartAuthentication (Authentication_Callback callback){

		#if UNITY_EDITOR
			_token = "3sfXSVeeyKwAAAAAAAAJO-BSICNhdYrmbhziIdRx7I2WWY72qbYRdtAzi6ZQji4x";
		#elif UNITY_ANDROID
			AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
			currentActivity.Call("start_Dropbox_Authentication");
		#endif
		_authen_callback = callback;
		Initalize ();
	}
	/*
	bool SaveTokenOnLocal (string token){
		string savePath = Application.persistentDataPath + "/bDropboxToken.bytes";
		try{
			using(var w = new BinaryWriter(File.OpenWrite(savePath))){
				w.Write(token); 
				w.Close();
				return true;
			}
		}catch{
			return false;
		}
	}

	string LoadTokenFromLocal(){
		string loadPath = Application.persistentDataPath + "/bDropboxToken.bytes";
		string token = "";
		using(var r = new BinaryReader(File.Open (loadPath, FileMode.Open))){
			token = r.ReadString(); 
		}
		return token;
	}
	*/

	public override void Update (){

		if (!_isGetToken) {

			#if UNITY_EDITOR
				
			#elif UNITY_ANDROID
				AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
				AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
				_token = currentActivity.Call<string> ("getTokenFromNative");

			#endif

			if(_token.Length > 0){
				_isGetToken = true;
				//SaveTokenOnLocal(_token);
				_authen_callback();
			}

		}

		/* Message System */
		if (_isUpdateListDone) {
			_updateList_callback (_updateList_result);
			_isUpdateListDone = false;
		}

		if (_isDownloadFileDone) {
			_downloadFile_callback ();
			_isDownloadFileDone = false;
		}

		if (_isDownloadMultipleFilesDone) {
			_downloadMultipleFile_callback ();
			_isDownloadMultipleFilesDone = false;
		}
	}

	public override string GetRecentPath (){
		return _recentPath;
	}
		
	public override bool GetCurrParentFileList ( fileList_Callback callback){ 
		if (_status == JobStatus.Started)
			return false;
		else
			_status = JobStatus.Started;

		/* "/" is root folder */
		if (_recentPath == "/") {
			return false;
		}

		string[] stringSeparators = new string[] {"/"};
		string[] result = _recentPath.Split(stringSeparators, StringSplitOptions.None);
		//example
		//recentFolder = "aaa/bbb/ccc"
		//cut it = "aaa/bbb/"
		for (int i = 0; i < (result.Length -1 ); ++i) {
			if (i == 0) {
				_recentPath = result [0];
			} else {
				_recentPath += "/" + result [i];
			}
		

		}

		bool res = GetFileListFromPath_internal (_recentPath, callback);
		if (res)
			return true;
		else
			return false;
	}

	public override bool GetSelectedFolderFileList(string _selectedFolderName, fileList_Callback callback){
		if (_status == JobStatus.Started)
			return false;
		else
			_status = JobStatus.Started;

		if (_recentPath == "/") {
			_recentPath += _selectedFolderName;
		} else {
			_recentPath += "/" + _selectedFolderName;
		}
			
		bool res = GetFileListFromPath_internal (_recentPath, callback);
		if (res)
			return true;
		else
			return false;
	}

	public override bool GetFileListFromPath (string path, fileList_Callback callback){
		if (_status == JobStatus.Started)
			return false;
		else
			_status = JobStatus.Started;

		bool res = GetFileListFromPath_internal (path, callback);
		if (res)
			return true;
		else
			return false;
	}
		
	public override bool DownloadFile (string filename, string savePath, string saveName, fileDownload_Callback callback){
		if (_status == JobStatus.Started)
			return false;
		else
			_status = JobStatus.Started;

		bool res = DownloadFile_internal (filename, savePath, saveName, callback);
		if (res)
			return true;
		else
			return false;
	}
		
	public override bool DonwloadAllFilesInFolder(string loadFolderPath, string saveFolderPath, fileDownload_Callback callback, fileDownload_Process_Callback proceed_callback){
		if (_status == JobStatus.Started)
			return false;
		else
			_status = JobStatus.Started;

		bool res = DonwloadAllFilesInFolder_internal (loadFolderPath, saveFolderPath, callback, proceed_callback);
		if (res)
			return true;
		else
			return false;
	}

	public override void JobDone (){
		_status = JobStatus.Done;
	}

	//Internal Usage----------------------------------------------------------------------------------------------
	private bool GetFileListFromPath_internal (string path, fileList_Callback callback){
		if (_updateList_bw.IsBusy != true) {
			_recentPath = path;
			_isUpdateListDone = false;
			_updateList_callback = callback;
			_updateList_bw.RunWorkerAsync (new updateList_argData(_token,CheckPathFilter (path) ));
			return true;
		} else {
			return false;
		}
	}
		
	private bool DownloadFile_internal(string filename, string savePath, string saveName, fileDownload_Callback callback){
		if (_downloadFile_bw.IsBusy != true) {
			_isDownloadFileDone = false;
			_downloadFile_callback = callback;

			_downloadFile_bw.RunWorkerAsync (new downloadFile_argData(_token,filename,savePath, saveName ));
			return true;
		} else {
			return false;
		}
	} 
		
	private bool DonwloadAllFilesInFolder_internal(string loadFolderPath, string saveFolderPath, fileDownload_Callback callback,fileDownload_Process_Callback proceed_callback){

		if (_downloadFile_bw.IsBusy == true || _updateList_bw.IsBusy == true) {
			return false;
		}

		_isDownloadMultipleFilesDone = false;

		_recentSaveFolderPath = saveFolderPath;
		_downloadMultipleFile_callback = callback;

		if (_recentPath != loadFolderPath) {
			GetFileListFromPath_internal (loadFolderPath, delegate(string resJson) {
				_updateList_result = resJson;
				DonwloadAllFilesInFolder_internal(_recentPath, _recentSaveFolderPath, _downloadMultipleFile_callback, proceed_callback);
			});
			return true;
		}

		if (!Directory.Exists (saveFolderPath)) {
			Directory.CreateDirectory(saveFolderPath);
		}


		JSONNode parseResult = JSON.Parse(_updateList_result);

		if(proceed_callback != null)
		proceed_callback (parseResult ["entries"].Count, processIdx);


		if (processIdx < parseResult ["entries"].Count) {
			//Ignore the folder
			if (parseResult ["entries"] [processIdx] [".tag"].Value == "file") {
				DownloadFile_internal (parseResult ["entries"] [processIdx] ["id"].Value, saveFolderPath, parseResult ["entries"] [processIdx] ["name"].Value, delegate() {
					processIdx++;
					DonwloadAllFilesInFolder_internal(_recentPath, _recentSaveFolderPath, _downloadMultipleFile_callback, proceed_callback);
				});
			} else {
				processIdx++;
				DonwloadAllFilesInFolder_internal(_recentPath, _recentSaveFolderPath, _downloadMultipleFile_callback, proceed_callback);
			}

		} else {
			/*Done*/
			_isDownloadMultipleFilesDone = true;
			processIdx = 0;
		}

		return true;
	}

	/*
	private void DonwloadAllFilesInFolder_refreshCallback(string json){
		_updateList_result = json;
		DonwloadAllFilesInFolder_internal(_recentPath, _recentSaveFolderPath, _downloadMultipleFile_callback);
	}
	private void DonwloadAllFilesInFolder_SignleFileDownloadDoneCallback(){
		DonwloadAllFilesInFolder_internal(_recentPath, _recentSaveFolderPath, _downloadMultipleFile_callback);
	}
	*/
		
	private void Initalize(){
		
		//Get Filelist background worker initalize
		_updateList_bw = new BackgroundWorker {
			WorkerReportsProgress = true,
			WorkerSupportsCancellation = true
		};

		_updateList_bw.DoWork += bw_getFileList_do;
		_updateList_bw.RunWorkerCompleted += bw_getFileList_done;

		//Download file backgroundWorker
		_downloadFile_bw = new BackgroundWorker {
			WorkerReportsProgress = true,
			WorkerSupportsCancellation = true
		};
		_downloadFile_bw.DoWork += bw_DownloadFilesFromPath_do;
		_downloadFile_bw.RunWorkerCompleted += bw_DownlaodFilesFromPath_done;

		//Authentication
		/*
		if (File.Exists (Application.persistentDataPath + "/bDropboxToken.bytes")) {
			_token = LoadTokenFromLocal();
			_isGetToken = true;
			_authen_callback();
		}
		*/
	}
		
	private void bw_DownloadFilesFromPath_do(object sender,  DoWorkEventArgs e){
		downloadFile_argData arg = (downloadFile_argData)e.Argument;  

		ServicePointManager.ServerCertificateValidationCallback = Validator;
		WebRequest request = WebRequest.Create("https://content.dropboxapi.com/2/files/download");
		request.Method = "POST";
		request.Headers.Add("Authorization: Bearer " + arg._token);
		//request.Headers.Add("Dropbox-API-Arg: {\"path\": \"/"+ arg._filePath + "\"}" );
		request.Headers.Add("Dropbox-API-Arg: {\"path\": \"" + arg._filePath + "\"}" );

		WebResponse response = request.GetResponse ();
		Stream responseStream = response.GetResponseStream();

		MemoryStream memoryStream = new MemoryStream ();

		byte[] buffer = new byte[response.ContentLength];
		byte[] result = null;
		int count = 0;
		do
		{
			count = responseStream.Read(buffer, 0, buffer.Length);
			memoryStream.Write(buffer, 0, count);

			if (count == 0) { break; }
		}
		while (true);

		result = memoryStream.ToArray();
			
		FileStream fs = new FileStream (arg._savePath + "/" + arg._saveName, FileMode.Create);
		fs.Write(result, 0, result.Length);
	}

	private void bw_DownlaodFilesFromPath_done(object sender, RunWorkerCompletedEventArgs e){
		if (e.Cancelled == true){
			//resultLabel.Text = "Canceled!";
		}else if (e.Error != null){
			//resultLabel.Text = "Error: " + e.Error.Message;
		}else{
			_isDownloadFileDone = true;
		}
	}
		
	private void bw_getFileList_do (object sender,  DoWorkEventArgs e){
		updateList_argData arg = (updateList_argData)e.Argument;  

		ServicePointManager.ServerCertificateValidationCallback = Validator;
		WebRequest request = WebRequest.Create("https://api.dropboxapi.com/2/files/list_folder");
		request.Method = "POST";

		request.Headers.Add("Authorization: Bearer " + arg._token);
		//        request.Headers.Add("Content-Type: applicatoin/json");
		request.ContentType = "application/json; charset=UTF-8";

		string postData = "{\"path\": \""+ arg._path +"\",\"recursive\": false,\"include_media_info\": false,\"include_deleted\": false}";
		byte[] byteArray = Encoding.UTF8.GetBytes(postData);
		request.ContentLength = byteArray.Length;

		Stream dataStream = request.GetRequestStream();
		dataStream.Write(byteArray, 0, byteArray.Length);

		WebResponse response = request.GetResponse();
		dataStream = response.GetResponseStream();

		StreamReader reader = new StreamReader(dataStream);
		string resultJSON = reader.ReadToEnd();

		reader.Close();
		dataStream.Close();
		response.Close();

		e.Result = resultJSON;
	}

	private void bw_getFileList_done (object sender, RunWorkerCompletedEventArgs e){
		if (e.Cancelled == true){
			//resultLabel.Text = "Canceled!";
		}else if (e.Error != null){
			//resultLabel.Text = "Error: " + e.Error.Message;
		}else{
			//resultLabel.Text = "Done!";
			_updateList_result = e.Result.ToString();
			_isUpdateListDone = true;
		}
	}

	private string CheckPathFilter(string path){
		if (path == "/" || path == "") {
			return "";
		} else {
			return path;
		}
	}

	public override string GetAPItoken(){
		return _token;
	}


}
