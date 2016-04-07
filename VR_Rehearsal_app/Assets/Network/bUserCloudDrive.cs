using UnityEngine;
using System.Collections;


//Adapter Design pattern
public class bUserCloudDrive  {

    enum eCloudType{
       NotSelected, bCloudDrive, GoogleDrive
    };

    bhClowdDriveAPI _bDriveAPI;
    bGoogleDriveAPI _bGoogleAPI;
  
    eCloudType _type = eCloudType.NotSelected;

    public void Setup(bGoogleDriveAPI googleDrive) {
        _bGoogleAPI = googleDrive;
    }
        
  public void Initialize(int type){
        if (_bGoogleAPI == null) {
#if UNITY_EDITOR
            Debug.LogError("Call Setup First !!");
#endif
        }

        switch (type) {
		case 1:
			_type = eCloudType.bCloudDrive;
				_bDriveAPI = new bDropboxAPI ();
                break;
            case 2:
                _type = eCloudType.GoogleDrive;
                break;
            case 3:
                _type = eCloudType.bCloudDrive;
				 _bDriveAPI = new bLocalDriveAPI();
                break;
        }
    }

    public void StartAuthentication(bhClowdDriveAPI.Authentication_Callback callback) {
        if (_type == eCloudType.NotSelected)
            return;

        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
                    _bDriveAPI.StartAuthentication(callback);
                }
                break;

            case eCloudType.GoogleDrive:
                {
                    _bGoogleAPI.StartAuthentication(callback);

                }
                break; 
        }
    }

    public bool GetFileListFromPath(string path, bhClowdDriveAPI.fileList_Callback callback) {
        if (_type == eCloudType.NotSelected)
            return false;

        bool res = false;
        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {   
                    res = _bDriveAPI.GetFileListFromPath(path, callback);
                }
                break;

            case eCloudType.GoogleDrive:
                {
                    res = true;
                    string resPath = path;
                    if (path == "/") {
                        resPath = "";
                    }
                       
                    _bGoogleAPI.GetSelectedFolderFileList(resPath, callback);
                }
                break;
        }
        return res;
    }

    public void JobDone() {
        if (_type == eCloudType.NotSelected)
            return;

        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
                    _bDriveAPI.JobDone();
                }
                break;

            case eCloudType.GoogleDrive:
                {

                }
                break;
        }
    }

    public bool GetCurrParentFileList(bhClowdDriveAPI.fileList_Callback callback) {
        if (_type == eCloudType.NotSelected)
            return false;

        bool res = false;
        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
                    res = _bDriveAPI.GetCurrParentFileList(callback);
                }
                break;

            case eCloudType.GoogleDrive:
                {
                    res = true;
                    _bGoogleAPI.GetCurrParentFileList(callback);
                }
                break;
        }
        return res;
    }

    public bool GetSelectedFolderFileList(string _selectedFolderName, bhClowdDriveAPI.fileList_Callback callback) {
        if (_type == eCloudType.NotSelected)
            return false;

        bool res = false;
        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
                    res = _bDriveAPI.GetSelectedFolderFileList(_selectedFolderName, callback);
                }
                break;

            case eCloudType.GoogleDrive:
                {
                    res = true;
                    _bGoogleAPI.GetSelectedFolderFileList(_selectedFolderName, callback);
                }
                break;
        }
        return res;
    }
    //public  bool DownloadFile(string filename, string savePath, string saveName, bhClowdDriveAPI.fileDownload_Callback callback);
	public bool DonwloadAllFilesInFolder(string loadFolderPath, string saveFolderPath, bhClowdDriveAPI.fileDownload_Callback callback, bhClowdDriveAPI.fileDownload_Process_Callback proceed_callback, bhClowdDriveAPI.fileDownload_Cancel_Callback cancel_callback) {
        if (_type == eCloudType.NotSelected)
            return false;

        bool res = false;
        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
				res = _bDriveAPI.DonwloadAllFilesInFolder(loadFolderPath, saveFolderPath, callback, proceed_callback, cancel_callback);
                }
                break;

            case eCloudType.GoogleDrive:
                {
                    res = true;
                    string resPath = loadFolderPath;
                    if (loadFolderPath == "/")
                    {
                        resPath = "";
                    }
				_bGoogleAPI.FileDownloadAll(loadFolderPath, saveFolderPath, callback, proceed_callback, cancel_callback);
                }
                break;
        }
        return res;
    }

    public void Update() {
        if (_type == eCloudType.NotSelected)
            return;

        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
                    _bDriveAPI.Update();
                }
                break;

            case eCloudType.GoogleDrive:
                {

                }
                break;
        }
    }

    //public  string GetAPItoken();
    public string GetRecentPath() {
        if (_type == eCloudType.NotSelected)
            return "none";

        string res = "none";
        switch (_type)
        {
            case eCloudType.bCloudDrive:
                {
                    res = _bDriveAPI.GetRecentPath();
                }
                break;

            case eCloudType.GoogleDrive:
                {
                    res = _bGoogleAPI.GetRecentPath();
                }
                break;
        }
        return res;
    }


	public void Revoke(){
		if (_type == eCloudType.NotSelected)
			return ;

		switch (_type)
		{
			case eCloudType.bCloudDrive:
				{
					_bDriveAPI.Revoke();
				}
				break;

			case eCloudType.GoogleDrive:
				{
					_bGoogleAPI.Revoke ();
				}
				break;
		}
		return;
	}

	public void CancelDownload(){
		if (_type == eCloudType.NotSelected)
			return ;

		switch (_type)
		{
		case eCloudType.bCloudDrive:
			{
				_bDriveAPI.CancelDownload();
			}
			break;

		case eCloudType.GoogleDrive:
			{
				_bGoogleAPI.CancelDownload ();
			}
			break;
		}
		return;
	}

    
}
