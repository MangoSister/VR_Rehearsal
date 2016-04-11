using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
