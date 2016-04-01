
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections.Generic;
using System;

public class NavigationView : MonoBehaviour {
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
    private string empty = "";
    private bool isReseting = false;
    private bool isCopy = false;
    public GameObject customView;

     //Download button components
    public GameObject progressCircle;
    public GameObject loadingView;
    public GameObject downloadButton;
    public Sprite[] thumbnails;  // 0 folder, 1 files , 2 pics.

    //Customize Data
    string showCaseName;
    int audience;
    int roomSize;
    int timer;

    


    void Start() {
		ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
        originalRect = contentRect.offsetMin.y;
        GetComponent<RectTransform>().SetAsLastSibling();
        isNavigationDone = false;
        _initialScrollContentSize = new Vector2(contentRect.rect.height, contentRect.rect.width);
        _bType = bType;
    }
	
	// Update is called once per frame
	void Update () {
        
        if (_userDrive != null)
        {
            _userDrive.Update();
        }
        if (isReseting == false)
        {
            ButtonListener();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameObject.activeSelf && _userDrive.GetRecentPath() == "/")
            {
                // go back fileTransfer
            }
            else if (gameObject.activeSelf && _userDrive.GetRecentPath() == empty)
            { 
                // go back fileTransfer
            }
            else {
                _userDrive.GetCurrParentFileList(delegate (string resJson)
                {
                    isReseting = true;
                    if (storedButton.Count != 0)
                    {
                        DeletePanels(true, "dd");
                    }
                    _userDrive.JobDone();
                    CreatePanels(resJson);
                    isReseting = false;
                });
            }
        }
    }
    public string RecentPath()
    {
        return _userDrive.GetRecentPath();
    }
    public void SetSetupManager(SetupManager mg)
    {
        _setManager = mg;
    
    }
 
    
    public void SetupCloud(int cloudType)
    {
        Debug.Log("cloudType" + cloudType);
        if (_userDrive == null)
            _userDrive = new bUserCloudDrive();

        _userDrive.Setup(_googleDirve);
        _userDrive.Initialize(cloudType);
        _userDrive.StartAuthentication(delegate ()
        {
            _userDrive.GetFileListFromPath("/", CreatePanels);
        });

    }
    public void CreateButtons(string _folder)
    {
        _userDrive.GetSelectedFolderFileList(_folder, delegate (string resJson)
        {
            isReseting = true;
            if (storedButton.Count != 0)
            {
                DeletePanels(true, "dd");
            }
            _userDrive.JobDone();
            CreatePanels(resJson);
            isReseting = false;
        });
    }

    public void UpdateButtons(string _folderName)
    {
        _userDrive.GetSelectedFolderFileList(_folderName, CreatePanels);
    }

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
        isCopy = false;
    }

    public void CreatePanels(string fileList)
    {

        JSONNode parseResult = JSON.Parse(fileList);
        GridLayoutGroup gLayout = canvasScroll.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout.cellSize.y;
        float spacing = gLayout.spacing.y;
        float totalSizeofRect = (cellSize) * parseResult["entries"].Count;
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
                string resFileName = parseResult["entries"][index]["name"].Value;
                string[] elements = resFileName.Split('.');
                string extentionFormat = elements[elements.Length - 1];
                Debug.Log("1 . File Name :" + resFileName + "L");
                Debug.Log("2 . ExtentionFormat :" + elements[elements.Length - 1] + "L");

                Debug.Log("3. Folder??:" + parseResult["entries"][index][".tag"].Value + "L");
                if (extentionFormat.Split(' ').Length - 1 > 0) {
                    extentionFormat = extentionFormat.Substring(0, extentionFormat.Length - 1);
                }
              
                if (extentionFormat == "jpg" || extentionFormat == "JPG" || extentionFormat == "png" || extentionFormat == "PNG" || extentionFormat == "Jpg" || extentionFormat == "Png")
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
                if (isCopy == false)
                {
                    if (GameObject.Find("PPT_Practice(Clone)(Clone)"))
                    {
                        Destroy(GameObject.Find("PPT_Practice(Clone)(Clone)"));
                    }
                    _selectedButton = Instantiate(_button) as GameObject;
                    isCopy = true;
                }
                if (_button.GetComponent<ButtonType>().buttonType == "folder")
                {
                    CreateButtons(_button.GetComponent<ButtonType>().buttonName);
                }

				_button.GetComponent<ButtonType> ().isSelected = false; 
            }
        }
    }

    public void DownloadButtonClicked()
    {
      
            try { 
            if (_selectedButton.GetComponent<ButtonType>().buttonType == "folder")
            {
                ShowLoadingPanel();
                string str = _userDrive.GetRecentPath();

                _pptID = _setManager.BShowcaseMgr.AddShowcase("empty", 0, "/empty", 30, 5);
                customView.GetComponent<CustomizeView>().SetPPTID(_pptID);
                _userDrive.DonwloadAllFilesInFolder(str, Application.persistentDataPath + "/" + _pptID, delegate ()
                {
                    Debug.Log("fileDownLoad Complete");
                    StartCoroutine("CompleteDownloading");
                    //  isNavigationDone = true;
                    //  gameObject.SetActive(false);
                    //   CustomizePanel();

                }, delegate (int totalFileNum, int completedFileNum)
                {
                    progressCircle.GetComponent<ProgressBar>().StartProgress(completedFileNum, totalFileNum);
                });
            }
            else
            {
                Debug.Log("you can;t download");
            }
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
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


}
