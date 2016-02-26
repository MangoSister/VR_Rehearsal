using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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

	private GameObject _logoPanel;
	private GameObject _loginPanel;
	private GameObject _listPanel;
	private GameObject _urlPanel;
    private GameObject _rotationPanel;
    

	private string _email;
	private string _url;
	private string _dbNumber;
	private string _comment;
	private string empty = "";

    public bool isRotate = false;
    
    
	void Start () {

		_logoPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("LogoPanel").gameObject;
		_loginPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("LoginPanel").gameObject;
		_listPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("ListPanel").gameObject;
		_urlPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("UrlPanel").gameObject;
        _rotationPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("RotationPanel").gameObject;
        ShowLogoPanel();
	}
	
	void Update () {
        if (isRotate == true)
        {
            IsRotate();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowListPanel();
        }
    }

	public void ShowLogoPanel(){
        _logoPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _logoPanel.SetActive(true);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(false);
		_urlPanel.SetActive(false);
        _rotationPanel.SetActive(false);
        StartCoroutine("ChangePanel");
	}

    public void ShowLoginPanel(){
        _loginPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _loginPanel.SetActive(true);
        _logoPanel.SetActive(false);
		
		_listPanel.SetActive(false);
		_urlPanel.SetActive(false);
        _rotationPanel.SetActive(false);
    }

    public void ShowListPanel(){
        _listPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _listPanel.SetActive(true);
        _logoPanel.SetActive(false);
		_loginPanel.SetActive(false);
		
		_urlPanel.SetActive(false);
        _rotationPanel.SetActive(false);
    }

    public void ShowUrlPanel(){
        _urlPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _urlPanel.SetActive(true);
        _logoPanel.SetActive(false);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(false);
		
        _rotationPanel.SetActive(false);
    }
    public void ShowRotation()
    {
        _rotationPanel.GetComponent<RectTransform>().SetAsLastSibling();
        _rotationPanel.SetActive(true);
        _logoPanel.SetActive(false);
        _loginPanel.SetActive(false);
        _listPanel.SetActive(false);
        _urlPanel.SetActive(false);
        isRotate = true;

    }
	public void OnSignInButtonClick(){
		//GameObject inputObject = GameObject.FindGameObjectWithTag("INPUT_EMAIL");
		//InputField inputField = inputObject.GetComponent<InputField>();
        ShowListPanel();
        /*
                if(inputField.text != empty){
                    ShowListPanel();
                }
                */
    }

	public void OnOkButtonClick(){
		InputField urlInputField = GameObject.FindGameObjectWithTag("INPUT_URL").GetComponent<InputField>();
		InputField dbInputField = GameObject.FindGameObjectWithTag("INPUT_DB").GetComponent<InputField>();
		InputField commentField = GameObject.FindGameObjectWithTag("INPUT_COMMENT").GetComponent<InputField>();

		if(urlInputField.text != empty || dbInputField.text != empty || commentField.text != empty){
            SetPowerPointData(commentField.text);
			ShowListPanel();
		}
	}
    public void OnPPTClick()
    {
        ShowRotation();
    }
    public void SetPowerPointData(string newStr)
    {
       GameObject pptPractice = (GameObject)_listPanel.GetComponent<RectTransform>().FindChild("PPT_Practice").gameObject;
       GameObject date = (GameObject)pptPractice.GetComponent<RectTransform>().FindChild("Date").gameObject;

        commentBox.GetComponent<Text>().text = string.Format("[{0}]", newStr); 
       
        Text dateText = date.GetComponent<Text>();
        dateText.text = string.Format("{0:yyyy.MM.dd HH:mm:ss}",System.DateTime.Now);
     
    }

	public void OnAddButtonClick(){
		ShowUrlPanel();
	}

	public IEnumerator ChangePanel(){
		yield return new WaitForSeconds(3.0f);
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
            // SceneManager.LaunchPresentationScene(new PresentationInitParam("sc_present_0"));
            // Application.LoadLevel("sc_present_0");
        }
        else if (Input.GetKeyDown(KeyCode.P)){
            prepHouse.GetComponent<PrepHouseKeeper>().NextScene();
        }
    }
}
