using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections.Generic;
public class UIManager : MonoBehaviour {

    /*  <UI Manager>
	 *  
	 *  In this scene, there are 4 Panels.
	 *  First,  LogoPanel shows only Logo.
	 *  Second, LoginPanel shows login inputfield and button (put email account) 
	 *  Thrid,	ListPanel shows list of ppt slides when user upload past slides and ppt karaoke
	 *  Fourth, UrlPanel shows 3 inputfields (Url, DB#, Comment parts). 
	 *  
	 *  <Process>
	 *  1. LogoPanel shows logo 3 seconds 
	 *  2. After 3 seconds, it will automatically change next Panel "LoginPanel"
	 *  3. In the LoginPanel, use rinput email account and click Login Button
	 *  4. If email account is valid, next will be ListPanel. ListPanel shows previous ppt slides and ppt Karaoke.
	 *  	1) when User click ppt Karaoke, Goes to gameplay scene directly. 
	 * 		2) when User click previous ppt slides, move next canvas "UrlPanel"
	 *  5. In the Panel, user put 3 data. URL, DB# and Comments.
	 *  6. After press Ok button, it start to download ppt. After finish the downloading, it will move to ListPanel again.
	 *  7. In the ListPanel, use rcan show latest ppt.
	 *  8. When use click ppt icon, goes to gameplay scene.
	 */
    #region variables
    public GameObject mainCanvas;
    public GameObject commentBox;
    public GameObject prepHouse;

    public GameObject logoCanvas;
    public GameObject showcaseCanvas;
    public GameObject connectCanvas;
    public GameObject urlCanvas;
    public GameObject rotationCanvas;
	public GameObject okButton;
    public GameObject navigationCanvas;
    public GameObject loadingCanvas;
    public GameObject customizeCanvas;
    
	private string empty = "";
    private InputField _urlInputField;
    private InputField _dbInputField;
    private InputField _commentField;

    private bhClowdDriveAPI bDriveAPI;
    private bShowcaseManager bShowcaseMgr;
    public  float offset;

    public GameObject CreateInstance;
    public GameObject prefab_ShowCase;

    public RectTransform RootRect;
    public RectTransform RootRect_ShowCase;
    public RectTransform RootCanvas;
    private List<GameObject> CreatedButton = new List<GameObject>();
    private List<GameObject> showCaseButtonList = new List<GameObject>();
    public List<GameObject> storedButton = new List<GameObject>();
    public List<GameObject> storedShocase = new List<GameObject>();
    private Vector2 InitialCanvasScrollSize;
    private float totalWidth = 0f;

    public bool isRotate = false;

    public ButtonType bType;
    private ButtonType _bType;
    public GameObject canvasScroll;
    public GameObject showCaseScroll;
    bool isButtonSelected = false;
    public Text token;
    private bool isReseting = false;
    private GameObject selectedButton;
    private bool isCopy = false;

    public GameObject ProgressCircle;

    #endregion

    void Start () {
        InitialCanvasScrollSize = new Vector2(RootRect.rect.height, RootRect.rect.width);
        //bDriveAPI = new bDropboxAPI();
        bShowcaseMgr = new bShowcaseManager();
        bShowcaseMgr.Start();

        //bDriveAPI.StartAuthentication();
        ShowLogoPanel();
        _bType = bType;
 	}
    
    void Update () {
  
        if (bDriveAPI != null)
        {
            bDriveAPI.Update();
        }

        if (isRotate == true)
        {
            IsRotate();
        }
        if (isReseting == false)
        {
            ButtonListener();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (navigationCanvas.activeSelf && bDriveAPI.GetRecentPath() == "/")
            {
                ShowConnectPanel();
            }
            else if (navigationCanvas.activeSelf && bDriveAPI.GetRecentPath() == empty)
            {
                ShowConnectPanel();
            }
            else if (connectCanvas.activeSelf)
            {
                ShowCasePanel();
            }
            else {
                bDriveAPI.GetCurrParentFileList(delegate (string resJson)
                {
                    isReseting = true;
                    if (storedButton.Count != 0)
                    {
                        DeletePanels__(true, "dd");
                    }
                    bDriveAPI.JobDone();
                    CreatePanels__(resJson);
                    isReseting = false;
                });
            }
        }

    }
    string pptID;
    public void SelectedDownload()
    {
        if (selectedButton.GetComponent<ButtonType>().buttonType =="folder")
        {
            ShowLoadingPanel();
            string str = bDriveAPI.GetRecentPath();

            pptID = bShowcaseMgr.AddShowcase("empty", 0, "/empty", 30,5);
            bDriveAPI.DonwloadAllFilesInFolder(str, Application.persistentDataPath +"/"+ pptID, delegate ()
            {
                Debug.Log("fileDownLoad Complete");
                // bShowcaseMgr.EditShowcase(id, showCaseName , 0,Application.persistentDataPath + "/" + id, (int)sliderVal);
               CustomizePanel();
                //bShowcaseMgr

            }, delegate(int totalFileNum, int completedFileNum) {
                ProgressCircle.GetComponent<ProgressBar>().StartProgress(completedFileNum, totalFileNum);
              //  Debug.Log("How many download = " + totalFileNum +"and also"+ completedFileNum);
            });
            Debug.Log("Folder : " + str + "path : " + Application.persistentDataPath);
        }
        else
        {
            Debug.Log("you can;t download");
        }
    }

    void ButtonListener()
    {
        foreach (GameObject _button in storedButton)
        {
            if(_button.GetComponent<ButtonType>().isSelected == true)// && isButtonSelected == false)
            {
                if (isCopy == false)
                {
                    if (GameObject.Find("PPT_Practice(Clone)(Clone)"))
                    {
                        Destroy(GameObject.Find("PPT_Practice(Clone)(Clone)"));
                    }
                    selectedButton = Instantiate(_button) as GameObject;
                    isCopy = true;
                }
                if (_button.GetComponent<ButtonType>().buttonType == "folder")
                {
                    CreateButtons(_button.GetComponent<ButtonType>().buttonName);
                }
            }
        }
        //isButtonSelected = false;
    }

    #region _Panel

    public void ShowLogoPanel(){
        logoCanvas.GetComponent<RectTransform>().SetAsLastSibling();
        logoCanvas.SetActive(true);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
        StartCoroutine("ChangePanel");
	}

    public void ShowCasePanel(){
        showcaseCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(true);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
        CheckLocalPPT();

    }

    public void ShowUrlPanel(){
		urlCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(true);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
    }

    public void ShowRotation()
    {
		rotationCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(true);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
        isRotate = true;
    }

    public void ShowConnectPanel()
    {
        connectCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(true);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
    }

    public void ShowNavigationPanel()
    {
        navigationCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(true);
        loadingCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
    }

    public void ShowLoadingPanel()
    {
        loadingCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        loadingCanvas.SetActive(true);
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        customizeCanvas.SetActive(false);
    }

    public void CustomizePanel()
    {
        customizeCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        customizeCanvas.SetActive(true);
        loadingCanvas.SetActive(false);
        logoCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
    }

    #endregion 

    public void CreateButtons(string _folder)
    {
        Debug.Log(bDriveAPI.GetRecentPath());
        bDriveAPI.GetSelectedFolderFileList(_folder, delegate (string resJson)
            {
                isReseting = true;
                // DeletePanels__(true, "dd");
                if (storedButton.Count != 0)
                {
                    DeletePanels__(true, "dd");
                }
                bDriveAPI.JobDone();
                CreatePanels__(resJson);
                isReseting = false;
            });
    }
    
    public void UpdateButtons(string _folderName) {
        bDriveAPI.GetSelectedFolderFileList(_folderName, CreatePanels__);
    }

    public void DeletePanels__(bool isSelected, string whichButton)
    {
        if (isSelected == true)
        {
            foreach (RectTransform child in RootRect)
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
   

    void StoreAllButtonStatus(GameObject button)
    {
        storedButton.Add(button);
    }

    public void CreatePanels__(string fileList)
    {
		var parseResult = JSON.Parse(fileList);
        GridLayoutGroup gLayout = canvasScroll.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout.cellSize.y;
        float span = gLayout.spacing.y;
        float totalSizeofRect = (cellSize- span) * parseResult["entries"].Count;
        RootRect.offsetMin = new Vector2(RootRect.offsetMin.x, -1 * (totalSizeofRect/2));
    //    // RootRect.offsetMax = new Vector2(RootRect.offsetMin.x, -10);
        //RootRect.offsetMax = new Vector2(RootRect.offsetMin.x, 0);
        for (int index = 0; index < parseResult["entries"].Count; index++)
        {
            GameObject createInstance = Instantiate(CreateInstance) as GameObject;

            createInstance.GetComponent<ButtonType>().buttonName = parseResult["entries"][index]["name"];
            createInstance.GetComponent<ButtonType>().buttonType = parseResult["entries"][index][".tag"];
            createInstance.GetComponentInChildren<Text>().text = parseResult["entries"][index]["name"];

            createInstance.transform.SetParent(RootRect, false);

            CreatedButton.Add(createInstance);
            StoreAllButtonStatus(createInstance);
        }
   
        bDriveAPI.JobDone();
        isButtonSelected = false;
    }

    public void OnPPTClick()
	{
        ShowRotation();
    }
    public void SetPowerPointData(string newStr)
    {
        GameObject pptPractice =showcaseCanvas.GetComponent<RectTransform>().FindChild("PPT_Practice").gameObject;
        GameObject date = (GameObject)pptPractice.GetComponent<RectTransform>().FindChild("Date").gameObject;
        commentBox.GetComponent<Text>().text = string.Format("[{0}]", newStr); 
        Text dateText = date.GetComponent<Text>();
        dateText.text = string.Format("{0:yyyy.MM.dd HH:mm:ss}",System.DateTime.Now);
     
    }

	public void OnAddButtonClick(){
        //ShowUrlPanel();
        ShowConnectPanel();
        //DeleteShowCase();
        
    }

	public IEnumerator ChangePanel(){
		yield return new WaitForSeconds(2.0f);
		ShowCasePanel();
	}

    void IsRotate()
    {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            bShowcaseMgr.End();
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            bShowcaseMgr.End();
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
  }
    public void DropboxClicked()
    {
        bDriveAPI = new bDropboxAPI();
        bDriveAPI.StartAuthentication(delegate() {
			bDriveAPI.GetFileListFromPath("/", CreatePanels__);
       });
   
        ShowNavigationPanel();
    }

    public int roomNumber;
    public void CheckToggle(int index)
    {
        switch(index)
        {
            case 0:
                Debug.Log("Fucking Large");
                roomNumber = 0;
                break;
            case 1:
                Debug.Log("Fucking Medi");
                roomNumber = 1;
                break;
            case 2:
                Debug.Log("Fucking small");
                roomNumber = 2;
                break;
            default:
                roomNumber = -1;
                break;
          }
    }

    public Slider sliderVla;
    public float sliderVal;
    public void CheckSliderValue()
    {
        sliderVal = sliderVla.value;
    }

    public InputField timer;
    public string time;
    public void SetTimer()
    {
        time = timer.text;
        Debug.Log("TIMER : " + time);
    }

    public InputField showCase;
    public string showCaseName;
    public void SetShowCaseName()
    {
        showCaseName = showCase.text;
        Debug.Log("ShowCase name : " + showCaseName);
    }
    public void TrueTest()
    {
        ShowRotation();
    }

    public void OkCustomize()
    {
      //  if(showCaseName != empty && roomNumber != -1 && time != empty)
       // {
            Debug.Log("PHAN!!");
            bShowcaseMgr.EditShowcase(pptID, showCaseName, 0, Application.persistentDataPath + "/" + pptID, (int)sliderVal, 5);
            ShowCasePanel();
		//}
        if (storedButton.Count > 0)
        {
            foreach (RectTransform child in RootRect)
            {
                if (child.name == "PPT_Practice(Clone)")
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            storedButton.Clear();
        }
        
    }

    void CheckLocalPPT()
    {
        DeleteShowCase();
        bShowcaseManager.showcase_Data[] caseDatas =  bShowcaseMgr.GetAllShowcases();
        GridLayoutGroup gLayout_showCase = showCaseScroll.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout_showCase.cellSize.y;
        float span = gLayout_showCase.spacing.y;
        float totalSizeofRect = (cellSize - span) * caseDatas.Length;
        for (int i = 0; i < caseDatas.Length; ++i) {
            Debug.Log(i + ": " + caseDatas[i]._showcaseID + "," + caseDatas[i]._showcaseName);
            GameObject createShowCase = Instantiate(prefab_ShowCase) as GameObject;
			createShowCase.GetComponent<ShowCaseButton>().SetData(caseDatas[i]._showcaseName, caseDatas[i]._mapIdx, caseDatas[i]._percentageOfAudience, caseDatas[i]._pptFolderPath, caseDatas[i]._showcaseID, 5);
            createShowCase.GetComponent<RectTransform>().FindChild("nameOfShowCase").GetComponent<Text>().text = caseDatas[i]._showcaseName;
            RootRect_ShowCase.offsetMin = new Vector2(RootRect_ShowCase.offsetMin.x, -1 * (totalSizeofRect / 2));
            createShowCase.transform.SetParent(RootRect_ShowCase, false);            
            showCaseButtonList.Add(createShowCase);
            StoreShowCaseButtons(createShowCase);
        }
    }
    void StoreShowCaseButtons(GameObject obj)
    {
        storedShocase.Add(obj);
    }
    void DeleteShowCase()
    {
        foreach (RectTransform child in RootRect_ShowCase)
        {
            if (child.name == "Prefab_ShowCase(Clone)")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        storedShocase.Clear();
    }
    
}
