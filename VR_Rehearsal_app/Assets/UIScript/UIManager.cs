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

	private GameObject _logoPanel;
	private GameObject _loginPanel;
	private GameObject _listPanel;
	private GameObject _urlPanel;
    

	private string _email;
	private string _url;
	private string _dbNumber;
	private string _comment;
	private string empty = "";

	void Start () {

		_logoPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("LogoPanel").gameObject;
		_loginPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("LoginPanel").gameObject;
		_listPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("ListPanel").gameObject;
		_urlPanel = (GameObject)mainCanvas.GetComponentInChildren<RectTransform>().FindChild("UrlPanel").gameObject;

		ShowLogoPanel();
	}
	
	void Update () {

	}

	void ShowLogoPanel(){
		_logoPanel.SetActive(true);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(false);
		_urlPanel.SetActive(false);
		StartCoroutine("ChangePanel");
	}

	void ShowLoginPanel(){
		_logoPanel.SetActive(false);
		_loginPanel.SetActive(true);
		_listPanel.SetActive(false);
		_urlPanel.SetActive(false);
	}

	void ShowListPanel(){
		_logoPanel.SetActive(false);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(true);
		_urlPanel.SetActive(false);
	}

	void ShowUrlPanel(){
		_logoPanel.SetActive(false);
		_loginPanel.SetActive(false);
		_listPanel.SetActive(false);
		_urlPanel.SetActive(true);
	}

	public void OnSignInButtonClick(){
		GameObject inputObject = GameObject.FindGameObjectWithTag("INPUT_EMAIL");
		InputField inputField = inputObject.GetComponent<InputField>();

		if(inputField.text != empty){
			ShowListPanel();
		}
	}

	public void OnOkButtonClick(){
		InputField urlInputField = GameObject.FindGameObjectWithTag("INPUT_URL").GetComponent<InputField>();
		InputField dbInputField = GameObject.FindGameObjectWithTag("INPUT_DB").GetComponent<InputField>();
		InputField commentField = GameObject.FindGameObjectWithTag("INPUT_COMMENT").GetComponent<InputField>();

		if(urlInputField.text != empty && dbInputField.text != empty && commentField.text != empty){
            SetPowerPointData(commentField.text);
			ShowListPanel();
		}
	}

    void SetPowerPointData(string newStr)
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
}
