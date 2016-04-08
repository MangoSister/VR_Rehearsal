using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FileTransferView : MonoBehaviour {

    public static bool isFileTransferViewDone;
    public int transferNumber = 0; //( 1 : dropbox, 2 : googleDrive, 3: USB)

    void Start () {
        GetComponent<RectTransform>().SetAsLastSibling();
        isFileTransferViewDone = false;
        transferNumber = 0;
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
		/*
		bool res = CheckForInternetConnection ();
		if (!res) {
			#if UNITY_EDITOR
			Debug.Log("Check Internet connection");
			#endif
			return;
		}
		*/
        transferNumber = 1;
        gameObject.SetActive(false);
        isFileTransferViewDone = true;
     }
    public void GoogleButtonClicked()
    {
		/*
		bool res = CheckForInternetConnection ();
		if (!res) {
			#if UNITY_EDITOR
			Debug.Log("Check Internet connection");
			#endif
			return;
		}
			
		*/
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
}
