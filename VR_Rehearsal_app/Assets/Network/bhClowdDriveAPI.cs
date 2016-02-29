using UnityEngine;
using System.Collections;

using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


public abstract class bhClowdDriveAPI{

	public enum JobStatus { NotStarted, Started, Done};

	public delegate void fileList_Callback(string filelists);
	public delegate void fileDownload_Callback();

	public struct bItem{
		string _name { get; set; }
		string _type { get; set; }
	}
	
	public abstract void StartAuthentication ();
	public abstract bool GetFileListFromPath (string path, fileList_Callback callback);
	public abstract bool GetCurrParentFileList( fileList_Callback callback);
	public abstract bool GetSelectedFolderFileList (string _selectedFolderName, fileList_Callback callback);

	public abstract bool DownloadFile (string filePath, string savePath,string saveName, fileDownload_Callback callback);
	public abstract bool DonwloadAllFilesInFolder(string loadFolderPath, string saveFolderPath,fileDownload_Callback callback);

	public abstract void JobDone ();

	protected abstract void GetAPItoken (string token);

	public abstract void Update (); 
	public abstract string GetRecentPath ();

	public bool Validator(object sender,X509Certificate certificate,X509Chain chain,SslPolicyErrors policyErrors){
		return true;
	}
}