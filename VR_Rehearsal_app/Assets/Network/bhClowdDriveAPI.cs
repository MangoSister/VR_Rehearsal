using UnityEngine;
using System.Collections;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


public abstract class bhClowdDriveAPI{

	public enum JobStatus { NotStarted, Started, Done};

	public delegate void Authentication_Callback(bool res);
	public delegate void fileList_Callback(string filelists);
	public delegate void fileDownload_Callback();

	public delegate void fileDownload_Cancel_Callback();

	public delegate void fileDownload_Process_Callback (int total, int proceedNumber);

	public abstract void StartAuthentication (Authentication_Callback callback);
	public abstract bool GetFileListFromPath (string path, fileList_Callback callback);
	public abstract bool GetCurrParentFileList( fileList_Callback callback);
	public abstract bool GetSelectedFolderFileList (string _selectedFolderName, fileList_Callback callback);

	public abstract bool DownloadFile (string filename, string savePath,string saveName, fileDownload_Callback callback);
	public abstract bool DonwloadAllFilesInFolder(string loadFolderPath, string saveFolderPath, fileDownload_Callback callback, fileDownload_Process_Callback proceed_callback , fileDownload_Cancel_Callback cancel_callback);

	public abstract void JobDone ();

	public abstract string GetAPItoken ();

	public abstract void Update (); 
	public abstract string GetRecentPath ();
	public abstract void Revoke ();


	public abstract void CancelDownload ();

	public bool Validator(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors policyErrors){
		return true;
	}



}