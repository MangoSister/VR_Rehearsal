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
    string deletedShowcaseID;
    void Start () {
        optionCover.GetComponent<Image>().color = gameObject.GetComponent<Button>().colors.normalColor;
    }
	
	// Update is called once per frame
	void Update () {
        /*
        RectTransform objectRectTransform = gameObject.GetComponent<RectTransform>();                // This section gets the RectTransform information from this object. Height and width are stored in variables. The borders of the object are also defined
        float width = objectRectTransform.rect.width;
        float height = objectRectTransform.rect.height;
        float rightOuterBorder = (width * .5f);
        float leftOuterBorder = (width * -.5f);
        float topOuterBorder = (height * .5f);
        float bottomOuterBorder = (height * -.5f);
        if (Input.mousePosition.x <= (transform.position.x + rightOuterBorder) && Input.mousePosition.x >= (transform.position.x + leftOuterBorder) && Input.mousePosition.y <= (transform.position.y + topOuterBorder) && Input.mousePosition.y >= (transform.position.y + bottomOuterBorder))
          {
          optionCover.GetComponent<Image>().color = gameObject.GetComponent<Button>().colors.highlightedColor;
         }
           else
          {
    
            
  
        }
        */
       

    }
    public void DeleteShowcaseButtonClicked()
    {
        deletedShowcaseID = _id;
        GameObject.Find("LocalCaseCanvas").GetComponent<LocalCaseView>().DeleteLocalShowcase(deletedShowcaseID);
    }
    public void OnShowCaseBUttonClicked()
    {
       // GameObject.Find("CanvasGroup").GetComponent<CanvasManager>().ShowRotationView();
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
