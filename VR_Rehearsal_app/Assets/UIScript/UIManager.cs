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
    public GameObject loginCanvas;
    public GameObject showcaseCanvas;
    public GameObject connectCanvas;
    public GameObject urlCanvas;
    public GameObject rotationCanvas;
	public GameObject okButton;
    public GameObject navigationCanvas;
    public GameObject loadingCanvas;
    

   	private string _email;
	private string _url;
	private string _dbNumber;
	private string _comment;
	private string empty = "";
    private InputField _urlInputField;
    private InputField _dbInputField;
    private InputField _commentField;

    private bhClowdDriveAPI bDriveAPI;
    private bShowcaseManager bShowcaseMgr;
    public  float offset;

    public GameObject CreateInstance;

    public RectTransform RootRect;
    public RectTransform RootCanvas;

    public RectTransform fixedButton;
    private List<GameObject> CreatedButton = new List<GameObject>();
    public List<GameObject> storedButton = new List<GameObject>();

    private Vector2 InitialCanvasScrollSize;
    private float totalWidth = 0f;

    public bool isRotate = false;

    public ButtonType bType;
    private ButtonType _bType;
    public GameObject canvasScroll;
    bool isButtonSelected = false;
    public Text token;
    private bool isReseting = false;
    private GameObject selectedButton;
    private bool isCopy = false;

    public GameObject ProgressCircle;

    
    #endregion

    void Start () {
        InitialCanvasScrollSize = new Vector2(RootRect.rect.height, RootRect.rect.width);
       
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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bDriveAPI.GetCurrParentFileList(delegate(string resJson) {
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

        if (isReseting == false)
        {
            ButtonListener();
        }
    }

    public void SelectedDownload()
    {
        if (selectedButton.GetComponent<ButtonType>().buttonType =="folder")
        {
            ShowLoadingPanel();
            string str = bDriveAPI.GetRecentPath();
            bDriveAPI.DonwloadAllFilesInFolder(str, Application.persistentDataPath , delegate ()
            {
                Debug.Log("fileDownLoad Complete");

            }, delegate(int totalFileNum, int completedFileNum) {
                ProgressCircle.GetComponent<ProgressBar>().StartProgress(completedFileNum, totalFileNum);
                Debug.Log("How many download = " + totalFileNum +"and also"+ completedFileNum);
            });

            Debug.Log("Folder : " + str + "path : " + Application.persistentDataPath);
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
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        StartCoroutine("ChangePanel");
	}

    public void ShowLoginPanel(){
        loginCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(true);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
    }

    public void ShowCasePanel(){
        showcaseCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(true);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
    }

    public void ShowUrlPanel(){
		urlCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(true);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
    }

    public void ShowRotation()
    {
		rotationCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(true);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
        isRotate = true;
    }

    public void ShowConnectPanel()
    {
        connectCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(true);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
        loadingCanvas.SetActive(false);
    }

    public void ShowNavigationPanel()
    {
        navigationCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(true);
        loadingCanvas.SetActive(false);
    }

    public void ShowLoadingPanel()
    {
        loadingCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        loadingCanvas.SetActive(true);
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        showcaseCanvas.SetActive(false);
        connectCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        navigationCanvas.SetActive(false);
    }

    #endregion 
    public void OnSignInButtonClick(){
        ShowCasePanel();
    }

    public void CreateButtons(string _folder)
    {
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
        //GameObject[] allPrefabs = GameObject.FindObjectOfType("PPT_Practice(Clone)");
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
       // Debug.Log("totalButton : "+storedButton.Count);
    }

    public void CreatePanels__(string fileList)
    {
        
        var parseResult = JSON.Parse(fileList);
        GridLayoutGroup gLayout = canvasScroll.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout.cellSize.y;
        float span = gLayout.spacing.y;
        float totalSizeofRect = (cellSize- span) * parseResult["entries"].Count;
        Debug.Log (parseResult["entries"].Count);
        RootRect.offsetMin = new Vector2(RootRect.offsetMin.x, -1 * (totalSizeofRect/2));
        // RootRect.offsetMax = new Vector2(RootRect.offsetMin.x, -10);
        RootRect.offsetMax = new Vector2(RootRect.offsetMin.x, 0);
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
       // isReseting = false;
    }
    
    public void OnOkButtonClick(){

        _urlInputField = GameObject.FindGameObjectWithTag("INPUT_URL").GetComponent<InputField>();
        _dbInputField = GameObject.FindGameObjectWithTag("INPUT_DB").GetComponent<InputField>();
        _commentField = GameObject.FindGameObjectWithTag("INPUT_COMMENT").GetComponent<InputField>();

        //if(urlInputField.text != empty || dbInputField.text != empty || commentField.text != empty){
        SetPowerPointData(_commentField.text);
        ShowCasePanel();
		//}
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

	}

	public IEnumerator ChangePanel(){
		yield return new WaitForSeconds(2.0f);
		ShowLoginPanel();
	}

	public void SetEmailAccount(string str){
		_email = str;
	}

	public string GetEmailAccount(){
		return _email;
	}

    void IsRotate()
    {
        if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
        {
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
  }
    public void DropboxClicked()
    {
        bDriveAPI = new bDropboxAPI();
        bDriveAPI.StartAuthentication();
        bDriveAPI.GetFileListFromPath("/", CreatePanels__);
        ShowNavigationPanel();
    }
}
