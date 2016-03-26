using UnityEngine;
using System.Collections;

public class ShowCaseButton : MonoBehaviour {
    private string _showCaseName;
    private int _sizeOfRoom;
    private int _numberOfAudience;
    private string _localPath;
    private string _id;
	private int _expectedTime;
    //UIManager uiManager;
    PrepHouseKeeper pHK;
  
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnShowCaseBUttonClicked()
    {
       // GameObject.Find("CanvasManager").GetComponent<UIManager>().ShowRotation();
        PresentationData.in_SlidePath = _localPath;
		// the unit of in_ExpectedTime is second
		PresentationData.in_ExpectedTime = _expectedTime * 60;
        switch (_sizeOfRoom) {
            case 0:
                PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                    break;
        }
        /*
         *  Send time, percentage of audience
         * 
         */

    }
    
    public void SetData(string showCanseName, int sizeOfRoom, int numberOfAudience, string localPath, string id, int _expectedTime)
    {
        _showCaseName = showCanseName;
        _sizeOfRoom = sizeOfRoom;
        _numberOfAudience = numberOfAudience;
        _localPath = localPath;
        _id = id;
		_expectedTime = _expectedTime;
    }
}
