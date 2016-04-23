using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FileTransferView : MonoBehaviour {

    public static bool isFileTransferViewDone;
    public int transferNumber = 0; //( 1 : dropbox, 2 : googleDrive, 3: USB)
	public GameObject warningTextUI_connection;
    public GameObject ok_connection;
    public GameObject instructionPanel;
    public Button xButon;
    public GameObject warningText;
    void Start () {
        instructionPanel.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.VisibleOverContent;
        GetComponent<RectTransform>().SetAsLastSibling();
        isFileTransferViewDone = false;
        transferNumber = 0;

		warningTextUI_connection.SetActive (false);
        warningText.SetActive(false);
    }

	private bool CheckForInternetConnection(){
		System.Net.WebClient client = null;
		System.IO.Stream stream = null;

		try{
			client = new System.Net.WebClient();
			stream = client.OpenRead("http://www.google.com");
			return true;
		}
		catch{
			return false;
		}
		finally{
			if (client != null) {
				client.Dispose ();
			}
			if (stream != null) {
				stream.Dispose ();
			}
		}
	
	}

	// Update is called once per frame
	public void DropboxButtonClicked()
    {
        
		bool res = CheckForInternetConnection ();
		if (!res) {
            ok_connection.SetActive(false);
			warningTextUI_connection.SetActive (true);
            warningText.SetActive(true);
			StartCoroutine(DelayForWarningMessage(1.0f));
			#if UNITY_EDITOR
			Debug.Log("Check Internet connection");
			#endif
			return;
		}
       
        transferNumber = 1;
        gameObject.SetActive(false);
        isFileTransferViewDone = true;
     }
    public void GoogleButtonClicked()
    {
		bool res = CheckForInternetConnection ();
		if (!res) {
            ok_connection.SetActive(false);
            warningTextUI_connection.SetActive (true);
            warningText.SetActive(true);
            StartCoroutine(DelayForWarningMessage(1.0f));
			#if UNITY_EDITOR
			Debug.Log("Check Internet connection");
			#endif

			return;
		}
        transferNumber = 2;
        gameObject.SetActive(false);
        isFileTransferViewDone = true;
    }
    public void USBButtonClicked()
    {
        transferNumber = 3;
        gameObject.SetActive(false);
        isFileTransferViewDone = true;
    }

	IEnumerator DelayForWarningMessage(float duration){
		yield return new WaitForSeconds (duration);
		warningTextUI_connection.SetActive (false);
        warningText.SetActive(false);
        ok_connection.SetActive(true);
    }

    public void QuestionButtonClick()
    {
        instructionPanel.SetActive(true);
    }
    public void XbuttonClick()
    {
        if (instructionPanel.activeSelf)
        {
            instructionPanel.SetActive(false);
        }
        else
        {
            xButon.GetComponent<Button>().interactable = false;
        }
    }
}
