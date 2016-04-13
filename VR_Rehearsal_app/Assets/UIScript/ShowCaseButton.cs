using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class ShowCaseButton : MonoBehaviour {
    private string _showCaseName;
    private int _sizeOfRoom;
    private int _numberOfAudience;
    private string _localPath;
    private string _id;
	private int _expectedTime;

    PrepHouseKeeper pHK;
    public GameObject optionCover;
	public bool isShowcaseButtonClicked;
    string deletedShowcaseID;
    public bool isCustomizeButtonClicked;
    public bool isShocaseButtonData;
  
	private GameObject _thumbnailObject;

    void Start () {
		isShowcaseButtonClicked = false;
        isShocaseButtonData = false;
        optionCover.GetComponent<Image>().color = gameObject.GetComponent<Button>().colors.normalColor;



			
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void SetData(string showCanseName, int sizeOfRoom, int numberOfAudience, string localPath, string id, int time)
    {
        _showCaseName = showCanseName;
        _sizeOfRoom = sizeOfRoom;
        _numberOfAudience = numberOfAudience;
        _localPath = localPath;
        _id = id;
        _expectedTime = time;

		LoadSlideThumbnail ();
    }

	public void LoadSlideThumbnail(){

		Transform[] ts = this.transform.GetComponentsInChildren<Transform> ();
		foreach (Transform t in ts) {
			if (t.gameObject.name == "thumbnail") {
				_thumbnailObject = t.gameObject;
			}
		}

		string[] imgNames_png = Directory.GetFiles(_localPath, "*.png", SearchOption.TopDirectoryOnly);
		string[] imgNames_jpg = Directory.GetFiles(_localPath, "*.jpg", SearchOption.TopDirectoryOnly);
		string[] imgNames_bmp = Directory.GetFiles(_localPath, "*.bmp", SearchOption.TopDirectoryOnly);
		string[] imgNames_PNG = Directory.GetFiles(_localPath, "*.PNG", SearchOption.TopDirectoryOnly);
		string[] imgNames_JPG = Directory.GetFiles(_localPath, "*.JPG", SearchOption.TopDirectoryOnly);
		string[] imgNames_BMP = Directory.GetFiles(_localPath, "*.BMP", SearchOption.TopDirectoryOnly);


		string[] imgNames = new string[imgNames_png.Length + imgNames_jpg.Length + imgNames_bmp.Length + imgNames_PNG.Length + imgNames_JPG.Length + imgNames_BMP.Length];

		System.Array.Copy(imgNames_png, imgNames, imgNames_png.Length);
		System.Array.Copy(imgNames_jpg, 0, imgNames, imgNames_png.Length, imgNames_jpg.Length);
		System.Array.Copy(imgNames_bmp, 0, imgNames, imgNames_jpg.Length, imgNames_bmp.Length);
		System.Array.Copy(imgNames_PNG, 0, imgNames, imgNames_bmp.Length, imgNames_PNG.Length);
		System.Array.Copy(imgNames_JPG, 0, imgNames, imgNames_PNG.Length, imgNames_JPG.Length);
		System.Array.Copy(imgNames_BMP, 0, imgNames, imgNames_JPG.Length, imgNames_BMP.Length);


		byte[] data = File.ReadAllBytes(imgNames[0]);
		Texture2D tempTexture = new Texture2D (1,1);
		tempTexture.LoadImage (data);

		Sprite thumbnail = Sprite.Create (tempTexture, new Rect (0f, 0f, tempTexture.width, tempTexture.height), new Vector2(0.5f, 0.5f));
		_thumbnailObject.GetComponent<Image> ().sprite = thumbnail;

	}
  
    public void OnShowCaseBUttonClicked()
    {
       // GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetData(_showCaseName, _sizeOfRoom,_numberOfAudience, _localPath, _id, _expectedTime);
        Debug.Log("ID  : " + _id);
        GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().SetPPTID(_id);
        isShowcaseButtonClicked = true;
        GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().DirectShowCalibrationView();

    }

    public void DeleteShowcaseButtonClicked()
    {
        deletedShowcaseID = _id;
        GameObject.Find("LocalCaseCanvas").GetComponent<LocalCaseView>().DeleteLocalShowcase(deletedShowcaseID);
    }
    public void CustomeButtonClicked()
    {
        isCustomizeButtonClicked = true;
        GameObject.Find("LocalCaseCanvas").GetComponent<LocalCaseView>().EditShowCase(_showCaseName, _sizeOfRoom,_numberOfAudience,_localPath, _id,_expectedTime);
    }
}
