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
	
	// Update is called once per frame
	public void DropboxButtonClicked()
    {
        transferNumber = 1;
        gameObject.SetActive(false);
        isFileTransferViewDone = true;
     }
    public void GoogleButtonClicked()
    {
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
