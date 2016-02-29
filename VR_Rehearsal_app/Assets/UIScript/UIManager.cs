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
    
	public GameObject mainCanvas;
    public GameObject commentBox;
    public GameObject prepHouse;

    public GameObject logoCanvas;
    public GameObject loginCanvas;
    public GameObject listCanvas;
    public GameObject urlCanvas;
    public GameObject rotationCanvas;
	public GameObject okButton;

    public RectTransform setTopButton;

   	private string _email;
	private string _url;
	private string _dbNumber;
	private string _comment;
	private string empty = "";
    private InputField _urlInputField;
    private InputField _dbInputField;
    private InputField _commentField;

    private bhClowdDriveAPI bDriveAPI;
    public  float offset;

    public GameObject CreateInstance;

    public RectTransform RootRect;
    public RectTransform RootCanvas;

    public RectTransform fixedButton;
    private List<GameObject> CreatedButton = new List<GameObject>();

    private Vector2 InitialCanvasScrollSize;
    private float totalWidth = 0f;

    public bool isRotate = false;

    public Vector3 StartAnim;
    public Vector3 EndAnim;

    public ButtonType bType;
    private ButtonType _bType;

    void Start () {
        InitialCanvasScrollSize = new Vector2(RootRect.rect.height, RootRect.rect.width);
        bDriveAPI = new bDropboxAPI();
        bDriveAPI.StartAuthentication();
        ShowLogoPanel();
        _bType = bType;
        Debug.Log("InitialCanvasScrollSize" + InitialCanvasScrollSize);
	}

  
    void Update () {
        bDriveAPI.Update();

        if (isRotate == true)
        {
            IsRotate();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowListPanel();
        }
    }

    #region _Panel

    public void ShowLogoPanel(){
        /*
        _logoPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _logoPanel.SetActive(true);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(false);
		_urlPanel.SetActive(true);
        _rotationPanel.SetActive(false);
        */
        logoCanvas.GetComponent<RectTransform>().SetAsLastSibling();
        logoCanvas.SetActive(true);
        loginCanvas.SetActive(false);
        listCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
        StartCoroutine("ChangePanel");
	}

    public void ShowLoginPanel(){/*
        _loginPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _logoPanel.SetActive(false);
		_loginPanel.SetActive(true);
    	_listPanel.SetActive(false);
		_urlPanel.SetActive(false);
        _rotationPanel.SetActive(false);
        */
        loginCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(true);
        listCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);

    }

    public void ShowListPanel(){
        /*
        _listPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _logoPanel.SetActive(false);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(true);
		_urlPanel.SetActive(false);
        _rotationPanel.SetActive(false);
        */
		listCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        listCanvas.SetActive(true);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(false);
    }

    public void ShowUrlPanel(){
        /*
        _urlPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _logoPanel.SetActive(false);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(false);
		_urlPanel.SetActive(true);
        _rotationPanel.SetActive(false);
        */
		urlCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        listCanvas.SetActive(false);
        urlCanvas.SetActive(true);
        rotationCanvas.SetActive(false);


    }
    public void ShowRotation()
    {
        /*
        _rotationPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _logoPanel.SetActive(false);
        _loginPanel.SetActive(false);
        _listPanel.SetActive(false);
        _urlPanel.SetActive(false);
        _rotationPanel.SetActive(true);
        */
		rotationCanvas.GetComponent<RectTransform>().SetAsFirstSibling();
        logoCanvas.SetActive(false);
        loginCanvas.SetActive(false);
        listCanvas.SetActive(false);
        urlCanvas.SetActive(false);
        rotationCanvas.SetActive(true);
        isRotate = true;

    }
    #endregion 
    public void OnSignInButtonClick(){
        //GameObject inputObject = GameObject.FindGameObjectWithTag("INPUT_EMAIL");
        //InputField inputField = inputObject.GetComponent<InputField>();

        
        ShowListPanel();
        CreateButtons("/");
        /*
                if(inputField.text != empty){
                    ShowListPanel();
                }
                */
    }
    public void CreateButtons(string _folder)
    {
        bDriveAPI.GetFileListFromPath(_folder, CreatePanels__);
    }

    public void Refresh()
    {
        bDriveAPI.GetFileListFromPath("/", CreatePanels__);
    }

    public void UpdateButtons(string _folderName) {
        bDriveAPI.GetSelectedFolderFileList(_folderName, CreatePanels__);
    }

    public void Download() {
        //string str = bDriveAPI.GetRecentPath();
        //bDriveAPI.DonwloadAllFilesInFolder(str, Application.persistentDataPath , delegate ()
        //{
        //    Debug.Log("fileDownLoad Complete");
        //});

        Debug.Log(Application.persistentDataPath);
    }

    

    private void CreatePanels__(string fileList)
    {
        Vector3 InstancePosition = EndAnim;
        Debug.Log(fileList);
        var parseResult = JSON.Parse(fileList);
     
        for (int index = 0; index < parseResult["entries"].Count; index++)
        {
            GameObject createInstance = Instantiate(CreateInstance) as GameObject;

            Text[] tempObject = createInstance.GetComponentsInChildren<Text>();
            foreach(Text go in tempObject) {
                if (go.name == "pptName") {
                    go.text = parseResult["entries"][index]["name"];
                } else if (go.name == "Date") {
                    go.text = parseResult["entries"][index][".tag"];
                }

            }


            createInstance.GetComponentInChildren<Text>().text = parseResult["entries"][index]["name"];
            
            //parseResult["entries"][index][".tag"]
            /*
            if("file")
            else if("folder")    
            */
            // + "/" + parseResult["entries"][index][".tag"];

            createInstance.transform.SetParent(RootRect, false);

            CreatedButton.Add(createInstance);

            RectTransform nextRect = setTopButton;
            offset = 3.5f * (nextRect.rect.height);
            CreatedButton[index].GetComponent<RectTransform>().localScale = setTopButton.localScale;
            CreatedButton[index].GetComponent<RectTransform>().position = new Vector2(nextRect.position.x, nextRect.position.y - offset);
            setTopButton = CreatedButton[index].GetComponent<RectTransform>();

          //  InstancePosition.y += offset;
            //totalWidth += offset;
        }
        bDriveAPI.JobDone();
    }

    

    public void OnOkButtonClick(){

        _urlInputField = GameObject.FindGameObjectWithTag("INPUT_URL").GetComponent<InputField>();
        _dbInputField = GameObject.FindGameObjectWithTag("INPUT_DB").GetComponent<InputField>();
        _commentField = GameObject.FindGameObjectWithTag("INPUT_COMMENT").GetComponent<InputField>();

        //if(urlInputField.text != empty || dbInputField.text != empty || commentField.text != empty){
        SetPowerPointData(_commentField.text);
			ShowListPanel();
		//}
	}
    public void OnPPTClick()
	{
        ShowRotation();
    }
    public void SetPowerPointData(string newStr)
    {
        GameObject pptPractice =listCanvas.GetComponent<RectTransform>().FindChild("PPT_Practice").gameObject;
        GameObject date = (GameObject)pptPractice.GetComponent<RectTransform>().FindChild("Date").gameObject;

        commentBox.GetComponent<Text>().text = string.Format("[{0}]", newStr); 
      
        Text dateText = date.GetComponent<Text>();
        dateText.text = string.Format("{0:yyyy.MM.dd HH:mm:ss}",System.DateTime.Now);
     
    }

	public void OnAddButtonClick(){
		ShowUrlPanel();
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

    void GetTypeFromButton()
    {
        string buttonType = _bType.returnType();
     //   Debug.Log("__BUTOTON" + buttonType);

    }
   
      

}
