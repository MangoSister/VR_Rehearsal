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
    //UIManager uiManager;
    PrepHouseKeeper pHK;
    public GameObject optionCover;
	public bool isShowcaseButtonClicked;
    string deletedShowcaseID;
    public bool isCustomizeButtonClicked;
    void Start () {
		isShowcaseButtonClicked = false;
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
        PresentationData.in_SlidePath = _localPath;
        PresentationData.in_ExpectedTime = _expectedTime;
		// the unit of in_ExpectedTime is second
		PresentationData.in_ExpectedTime = _expectedTime * 60;
        switch (_sizeOfRoom) {
            case 0:
                PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                    break;
            case 2:
                PresentationData.in_EnvType = PresentationData.EnvType.EmptySpace;
                break;

            case 3:
                PresentationData.in_EnvType = PresentationData.EnvType.ConferenceRoom;
                break;
          }
        isShowcaseButtonClicked = true;
        GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().DirectShowRotationView();

    }

    public void DeleteShowcaseButtonClicked()
    {
        deletedShowcaseID = _id;
        GameObject.Find("LocalCaseCanvas").GetComponent<LocalCaseView>().DeleteLocalShowcase(deletedShowcaseID);
    }
    public void CustomeButtonClicked()
    {
        Debug.Log("Customize Button Clicked");
        isCustomizeButtonClicked = true;
        GameObject.Find("LocalCaseCanvas").GetComponent<LocalCaseView>().EditShowCase(_showCaseName, _sizeOfRoom,_numberOfAudience,_localPath, _id,_expectedTime);

    }
}
