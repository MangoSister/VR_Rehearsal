
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections.Generic;
using System;

public class NavigationView : MonoBehaviour {

	enum NavigationStatus { Processing, NotProcessing }
	enum AuthCheck { Succeed, failed }

    // Dropbox API
    bUserCloudDrive _userDrive;
    public bGoogleDriveAPI _googleDirve;
    private SetupManager _setManager;
    string _pptID;

    //Canvas Status
    public static bool isNavigationDone;

    //Navigation folders
    private List<GameObject> _createdButton = new List<GameObject>();
    public List<GameObject> storedButton = new List<GameObject>();
  
    //ScrollRect components
    private Vector2 _initialScrollContentSize;
    public GameObject canvasScroll;
    public RectTransform contentRect;
    float originalRect;


    //Button components
    private ButtonType _bType;
    private GameObject _selectedButton;
    public ButtonType bType;
    public GameObject naviButtonPrefab;
    bool isButtonSelected = false;

    //ETC
    private string _empty = "";
    private bool _isReseting = false;
    private bool _isCopy = false;
    public GameObject customView;

     //Download button components
    public GameObject progressCircle;
    public GameObject loadingView;
   
    public Sprite[] thumbnails;  // 0 folder, 1 files , 2 pics.
    bool isOkToDown;


	public GameObject Icon_emptyFolder;
	public GameObject Button_Download;
	public GameObject Icon_AuthFailed;

    //Customize Data
	private string _showCaseName;
	private int _audience;
	private int _roomSize;
	private int _timer;
	private string _extentionFormat;

	AndroidJavaObject _currentActivity;

	//Authentication Check
	AuthCheck _authCheck = AuthCheck.failed;
	int _currCloudType = 0;
	float _timerForAuth = 0;

	//Loading
	bool _isLoading = false;
	public Sprite[] loadingAnimSprites;
	public GameObject loadingAnimPlaceholder;
	float _load_CR_timer = 0;

    //flag
    public bool letsdefault;
	NavigationStatus _NaviStatus= NavigationStatus.NotProcessing;

	//recent Json List
	JSONNode _recentParseResult;

    //MemoryCheck
    public GameObject warningMemoryPanel;

    void Start() {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.TranslucentOverContent;

        letsdefault = false;
        originalRect = contentRect.offsetMin.y;
        GetComponent<RectTransform>().SetAsLastSibling();
        isNavigationDone = false;
        _initialScrollContentSize = new Vector2(contentRect.rect.height, contentRect.rect.width);
        _bType = bType;
		_NaviStatus = NavigationStatus.NotProcessing;
		ResetIcons ();

		#if UNITY_EDITOR

		
		#elif UNITY_ANDROID
		AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		_currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");
		#endif
    }
	//Reset icons to be default mode
	void ResetIcons(){
		ResetSetting ();

		loadingAnimPlaceholder.SetActive (false);
		Icon_emptyFolder.SetActive(false);
		Button_Download.SetActive(false);
		Icon_AuthFailed.SetActive (false);
	}

	void ResetSetting(){
		 _timerForAuth = 0;
	}

	public void Initialize(){
		ResetIcons ();
		_NaviStatus = NavigationStatus.NotProcessing;
		_authCheck = AuthCheck.failed;
		_currCloudType = 0;

		isButtonSelected = false;

		//ETC
		_empty = "";
		_isReseting = false;
		_isCopy = false;

		FinishLoading ();
	}
	
	// Update is called once per frame
	void Update () {
        
		if(_authCheck == AuthCheck.failed){
			_timerForAuth += Time.deltaTime;

			if (_timerForAuth > 15.0f) {
				Icon_AuthFailed.SetActive (true);
			}
		}


        if (_userDrive != null)
        {
            _userDrive.Update();
        }
        if (_isReseting == false)
        {
            ButtonListener();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {	

			if (!this.gameObject.activeSelf || _NaviStatus == NavigationStatus.Processing)
				return;

			_NaviStatus = NavigationStatus.Processing;
			/*navigation back- go to parent paht*/
			if(_userDrive.GetRecentPath() != "/" || _userDrive.GetRecentPath() == _empty) {
				

                _userDrive.GetCurrParentFileList(delegate (string resJson)
                {
                    _isReseting = true;
					_NaviStatus = NavigationStatus.NotProcessing;

                    if (storedButton.Count != 0)
                    {
                        DeletePanels(true, "dd");
                    }
                    _userDrive.JobDone();
                    CreatePanels(resJson);
                    _isReseting = false;


                });
            }
        }
    }
    public string RecentPath()
    {
		if (_userDrive != null)
			return _userDrive.GetRecentPath ();
		else
			return "null";
    }

	public bool GetAuthenticationStatus(){
		if (_authCheck == AuthCheck.Succeed) {
			return true;
		}
		return false;
	}

    public void SetSetupManager(SetupManager mg)
    {
        _setManager = mg;
    }
 
    
    public void SetupCloud(int cloudType)
    {
		if (_NaviStatus == NavigationStatus.Processing)
			return;

		if (_userDrive == null)
            _userDrive = new bUserCloudDrive();


		_currCloudType = cloudType;
        _userDrive.Setup(_googleDirve);
        _userDrive.Initialize(cloudType);

		StartLoading ();

		_NaviStatus = NavigationStatus.Processing;
		_userDrive.StartAuthentication(delegate (bool res, int resCode)
        {
			FinishLoading();
			_NaviStatus = NavigationStatus.NotProcessing;

            if (res)
            {	
				if(resCode == 1){
						Debug.Log("Credential Error");
				}else{

					_userDrive.GetFileListFromPath("/", CreatePanels);
				}
				_authCheck = AuthCheck.Succeed;
				_timerForAuth = 0;
			}else{
				_authCheck = AuthCheck.failed;
				Icon_AuthFailed.SetActive(true);
			}


        });

    }
    public void CreateButtons(string _folder)
    {
		if (_NaviStatus == NavigationStatus.Processing)
			return;

		StartLoading ();

		_NaviStatus = NavigationStatus.Processing;
        _userDrive.GetSelectedFolderFileList(_folder, delegate (string resJson)
        {
			FinishLoading();
			_NaviStatus = NavigationStatus.NotProcessing;

            _isReseting = true;
            if (storedButton.Count != 0)
            {
                DeletePanels(true, "dd");
            }
            _userDrive.JobDone();
            CreatePanels(resJson);
            _isReseting = false;


        });
    }


	/*
    public void UpdateButtons(string _folderName)
    {
        _userDrive.GetSelectedFolderFileList(_folderName, CreatePanels);
    }*/

    void StoreAllButtonStatus(GameObject button)
    {
        storedButton.Add(button);
    }


    public void DeletePanels(bool isSelected, string whichButton)
    {
        if (isSelected == true)
        {
            foreach (RectTransform child in contentRect)
            {
                if (child.name == "PPT_Practice(Clone)")
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }
        storedButton.Clear();
		ResetIcons ();
        _isCopy = false;
    }

	public void ClearPanels(){
		foreach (RectTransform child in contentRect)
		{
			if (child.name == "PPT_Practice(Clone)")
			{
				GameObject.Destroy(child.gameObject);
			}
		}
		storedButton.Clear();

		//icon
		ResetIcons ();
	}


    public void CreatePanels(string fileList)
    {
        
        JSONNode parseResult = JSON.Parse(fileList);
		_recentParseResult = parseResult;
        GridLayoutGroup gLayout = canvasScroll.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout.cellSize.y;
        float spacing = gLayout.spacing.y;
        float totalSizeofRect = (cellSize) * parseResult["entries"].Count;

		CheckFileList (parseResult);

        //  contentRect.offsetMax = new Vector2(contentRect.offsetMin.x, 0.0f);
        if (parseResult["entries"].Count < 7)
        {
            contentRect.offsetMax = new Vector2(contentRect.offsetMin.x, -12f);
            contentRect.offsetMin = new Vector2(contentRect.offsetMin.x, originalRect);//contentRect.offsetMin.y);
        }
        else {
            contentRect.offsetMax = new Vector2(contentRect.offsetMin.x, -12f);
            contentRect.offsetMin = new Vector2(contentRect.offsetMin.x, (-1 * totalSizeofRect / 2) + ((spacing * parseResult["entries"].Count) / 3));
        }
        for (int index = 0; index < parseResult["entries"].Count; index++)
        {
            GameObject createInstance = Instantiate(naviButtonPrefab) as GameObject;

            createInstance.GetComponent<ButtonType>().buttonName = parseResult["entries"][index]["name"].Value;
            createInstance.GetComponent<ButtonType>().buttonType = parseResult["entries"][index][".tag"].Value;
            createInstance.GetComponentInChildren<Text>().text = parseResult["entries"][index]["name"].Value;
          
            if (parseResult["entries"][index][".tag"].Value == "folder") 
            {
                createInstance.GetComponent<RectTransform>().FindChild("thumbnail").GetComponent<Image>().sprite = thumbnails[0];

            }else {
				string _extentionFormat = GetFileExtentionFormat( parseResult["entries"][index]["name"].Value);
              
                if (_extentionFormat == "jpg" || _extentionFormat == "JPG" || _extentionFormat == "png" || _extentionFormat == "PNG" || _extentionFormat == "Jpg" || _extentionFormat == "Png")
                {
                    createInstance.GetComponent<RectTransform>().FindChild("thumbnail").GetComponent<Image>().sprite = thumbnails[2];
                }
                else
                {
                    createInstance.GetComponent<RectTransform>().FindChild("thumbnail").GetComponent<Image>().sprite = thumbnails[1];
                }
            }
            createInstance.transform.SetParent(contentRect, false);

            _createdButton.Add(createInstance);
            StoreAllButtonStatus(createInstance);
        }

        _userDrive.JobDone();
        isButtonSelected = false;
    }

	public void CheckFileList(JSONNode parseResult){

		bool isThereNoEntre = false;
		bool isDownloadable = false;
		for (int index = 0; index < parseResult ["entries"].Count; index++) {
			

			if (parseResult["entries"][index][".tag"].Value == "folder" || parseResult ["entries"] [index] [".tag"].Value == "file") {
				if (parseResult ["entries"] [index] [".tag"].Value == "file") {
					
					string _extentionFormatStr = GetFileExtentionFormat (parseResult ["entries"] [index] ["name"].Value);
					if (_extentionFormat == "jpg" || _extentionFormat == "JPG" || _extentionFormat == "png" || _extentionFormat == "PNG" || _extentionFormat == "Jpg" || _extentionFormat == "Png")
					{
						/* + Checking that there is downloadable files in the selected folder for Showing Downdload icon */
						isDownloadable = true;
					}
				}

				/* + Checking _empty or not in the selected folder for Showing _empty indication icon */
				isThereNoEntre = true;
			}
		
		}
			
		Icon_emptyFolder.SetActive(!isThereNoEntre);
		Button_Download.SetActive(isDownloadable);
	}

	string GetFileExtentionFormat(string fileName){
		string[] elements = fileName.Split('.');
		_extentionFormat = elements[elements.Length - 1];
		if (_extentionFormat.Split(' ').Length - 1 > 0) {
			_extentionFormat = _extentionFormat.Substring(0, _extentionFormat.Length - 1);
		}
		return _extentionFormat;
	}

    void ButtonListener()
    {
        foreach (GameObject _button in storedButton)
        {
            if(_button.GetComponent<ButtonType>().buttonType != "folder")
            {
                _button.GetComponent<Button>().interactable = false;
                            
            }
            if (_button.GetComponent<ButtonType>().isSelected == true )//&& isButtonSelected == false)
            {
                if (_isCopy == false)
                {
                    if (GameObject.Find("PPT_Practice(Clone)(Clone)"))
                    {
                        Destroy(GameObject.Find("PPT_Practice(Clone)(Clone)"));
                    }
                    _selectedButton = Instantiate(_button) as GameObject;
                    _isCopy = true;
                }
                if (_button.GetComponent<ButtonType>().buttonType == "folder")
                {
                    CreateButtons(_button.GetComponent<ButtonType>().buttonName);
                    return;
                }

				_button.GetComponent<ButtonType> ().isSelected = false; 
            }
        }
    }

	public void DownloadButtonClick_Pre(){


		customView.GetComponent<CustomizeView>().DefaultValueSetting();
	   
        /* banned root folder 
		if (!_selectedButton && !(_selectedButton.GetComponent<ButtonType>().buttonType == "folder") && !(_NaviStatus == NavigationStatus.Processing) )
			return;
       */
			
		StartLoading ();
		string str = _userDrive.GetRecentPath();
		_NaviStatus = NavigationStatus.Processing;
			
	
				
		JSONNode parseResult = _recentParseResult;

		long totalFileSize = 0;
		for (int index = 0; index < parseResult["entries"].Count; index++){
			if (parseResult["entries"][index][".tag"].Value == "folder") {
				continue;
	
			}else{
				string _extentionFormat = GetFileExtentionFormat( parseResult["entries"][index]["name"].Value);
				
				if (_extentionFormat == "jpg" || _extentionFormat == "JPG" || _extentionFormat == "png" || _extentionFormat == "PNG" || _extentionFormat == "Jpg" || _extentionFormat == "Png")
				{
					//Debug.Log(parseResult["entries"][index]["size"].Value);
					totalFileSize += Convert.ToInt64(parseResult["entries"][index]["size"].Value);
				}

			}
		}

		FinishLoading();
#if UNITY_EDITOR
		Debug.Log("Total File Size:" + totalFileSize);
		DownloadButtonClicked ();
#elif UNITY_ANDROID
		
		long currentAvailableMemorySize = _currentActivity.CallStatic<long> ("GetAvailableMemory", Application.persistentDataPath);
		Debug.Log("Available Memory:" + currentAvailableMemorySize);

		if(totalFileSize > currentAvailableMemorySize){
			/* ##Exceed File Memory for saving slides*/
			Debug.Log("File Exceed");
			ShowExceedMemory();

		}else{
			DownloadButtonClicked ();
		}
#endif



    }

    public void DownloadButtonClicked()
    {
        //customView.GetComponent<CustomizeView>().DefaultValueSetting();

        try
        {
            string str = _userDrive.GetRecentPath();

            _pptID = _setManager.BShowcaseMgr.AddShowcase("_empty", 0, "/_empty", 30, 5);
            _setManager.BShowcaseMgr.EditShowcase_path(_pptID, (Application.persistentDataPath + "/" + _pptID));
            customView.GetComponent<CustomizeView>().SetPPTID(_pptID);

			bool isProgressbarStart = false; 
                    _userDrive.DonwloadAllFilesInFolder(str, Application.persistentDataPath + "/" + _pptID,
                        delegate ()
                        {   /* completed Callback */
							#if UNITY_EDITOR
                            Debug.Log("fileDownLoad Complete");
							#endif
                            _userDrive.JobDone();
                            _NaviStatus = NavigationStatus.NotProcessing;
                            StartCoroutine("CompleteDownloading");
                        }, delegate (int totalFileNum, int completedFileNum) /* process Callback */
                        {
							ProgressBar progressBar =  progressCircle.GetComponent<ProgressBar>();
							
							if(completedFileNum == 0 && isProgressbarStart == false){
								isProgressbarStart = true;
								ShowLoadingPanel();
								progressBar.ResetProgress();	
							}
							progressBar.StartProgress(completedFileNum, totalFileNum);
                        }, delegate ()
                        {/* Cancel Callback */
							#if UNITY_EDITOR
                            Debug.Log("fileDownLoad Canceled");
							#endif
                            _setManager.BShowcaseMgr.DeleteShowcase(_pptID);
							
                            _userDrive.JobDone();
                            _NaviStatus = NavigationStatus.NotProcessing;
                            loadingView.SetActive(false);
                        });
                
                
            
        }
        catch (Exception e)
        {
            _userDrive.JobDone();
            _NaviStatus = NavigationStatus.NotProcessing;
            Debug.Log(e.ToString());
        }
    }
   

	public void CancelDownloadButton(){
		_userDrive.CancelDownload ();
	}

	public void ClickLogoutButton(){
		
		if (_authCheck == AuthCheck.Succeed) {

			if (_NaviStatus == NavigationStatus.Processing)
				return;	

			_NaviStatus = NavigationStatus.Processing;
			_userDrive.Revoke (delegate(){
				_NaviStatus = NavigationStatus.NotProcessing;

				if(_currCloudType != 0){
					ClearPanels();
					SetupCloud(_currCloudType);
                   
				}


			});
		}
	}

    public void ShowLoadingPanel()
    {
        loadingView.GetComponent<RectTransform>().SetAsFirstSibling();
        loadingView.SetActive(true);
     
    }
    IEnumerator CompleteDownloading()
    {
        yield return new WaitForSeconds(1.0f);
        isNavigationDone = true;
        gameObject.SetActive(false);
    } 

	void StartLoading(){
		_isLoading = true;
		loadingAnimPlaceholder.SetActive (true);
		StartCoroutine (StartLoadingIcon ());
	}
	void FinishLoading(){
		_isLoading = false;
		loadingAnimPlaceholder.SetActive (false);
	}

	IEnumerator StartLoadingIcon(){
		
		float timer = 0;
		int idx = 0;

		while(_isLoading){
			/* Animation */
			timer += Time.deltaTime;

			if (timer > 0.5f) {
				int tempIdx = (++idx) % loadingAnimSprites.Length;
				loadingAnimPlaceholder.GetComponent<Image>().sprite = loadingAnimSprites [tempIdx];
				timer = 0;
			}
				
			yield return null;
		}

	}

	void ShowExceedMemory(){
        warningMemoryPanel.SetActive(true);
	}
    public void MemoryWarningOkButtonClick()
    {
        warningMemoryPanel.SetActive(false);
        _NaviStatus = NavigationStatus.NotProcessing;
    }

}
