using UnityEngine;
using System.Collections;

public class ShowCaseButton : MonoBehaviour {
    private string _showCaseName;
    private int _sizeOfRoom;
    private int _numberOfAudience;
    private string _localPath;
    private string _id;
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
        GameObject.Find("CanvasManager").GetComponent<UIManager>().ShowRotation();
        //GlobalManager.EnterPresentation(_localPath,PresentationData.EnvType.RPIS);
        PresentationData.in_SlidePath = _localPath;

        switch (_sizeOfRoom) {
            case 0:
                PresentationData.in_EnvType = PresentationData.EnvType.RPIS;
                    break;
        }
       

    }
    
    public void SetData(string showCanseName, int sizeOfRoom, int numberOfAudience, string localPath, string id)
    {
        _showCaseName = showCanseName;
        _sizeOfRoom = sizeOfRoom;
        _numberOfAudience = numberOfAudience;
        _localPath = localPath;
        _id = id;
    }
}
