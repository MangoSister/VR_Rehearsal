using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class LocalCaseView : MonoBehaviour {

    public static bool isLocalCaseDone;
    private SetupManager _setManager;

    public List<GameObject> storedShowCase = new List<GameObject>();
    private List<GameObject> showCaseButtonList = new List<GameObject>();

    public GameObject canvasScroll;
    public RectTransform showCaseContentRect;
    float originalRect;
    float finalRect;

    public GameObject showCasePrefab;
    public GameObject contentRect;

    public bool isFileTransferClicked = false;
    public static bool isCustomizeButtonClicked;
    public GameObject customView;
    int totalShowcase;
    
    int deleteCount = 0;

    public GameObject emptyIcon;

    void Start () {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        isLocalCaseDone = false;
        isCustomizeButtonClicked = false;
        if (deleteCount == 0)
        {
            CheckLocalPPT();
        }
        else
        {
           if(totalShowcase > 7)
            {
                CheckLocalPPT();
            }
            else
            {
                GridLayoutGroup gLayout_showCase = contentRect.GetComponent<GridLayoutGroup>();
                gLayout_showCase.padding.top = -580;
                CheckLocalPPT();
            }
        }
        if (storedShowCase.Count == 0)
        {
            emptyIcon.SetActive(true);
        }
        else
        {
            emptyIcon.SetActive(false);
        }
        GetComponent<RectTransform>().SetAsLastSibling();
    }
	
	void Update () {
    }

    public void SetSetupManager(SetupManager mg)
    {
        _setManager = mg;
    }
    void CheckLocalPPT()
    {
        DeleteShowCase();
        isLocalCaseDone = true;
        bShowcaseManager.showcase_Data[] caseDatas = _setManager.BShowcaseMgr.GetAllShowcases();
        if (caseDatas == null)
        {
            return;
        }
        GridLayoutGroup gLayout_showCase = contentRect.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout_showCase.cellSize.y;
        float span = gLayout_showCase.spacing.y;
        float totalSizeofRect = (cellSize - (span/2)) * caseDatas.Length;
        finalRect = -(originalRect + (totalSizeofRect / 2));
        totalShowcase = caseDatas.Length;
        if (caseDatas!=null)
        {
            if (caseDatas.Length < 7)
            {
                gLayout_showCase.padding.top = -580;
                showCaseContentRect.offsetMax = new Vector2(showCaseContentRect.offsetMin.x, -12f);
                showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, originalRect);
            }
            else
            {
                gLayout_showCase.padding.top = -68;
                showCaseContentRect.offsetMax = new Vector2(showCaseContentRect.offsetMin.x, -12f);
                showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, finalRect);
            }

            for (int i = 0; i < caseDatas.Length; ++i)
            {
                GameObject createShowCase = Instantiate(showCasePrefab) as GameObject;
                createShowCase.GetComponent<ShowCaseButton>().SetData(caseDatas[i]._showcaseName, caseDatas[i]._mapIdx, caseDatas[i]._percentageOfAudience, caseDatas[i]._pptFolderPath, caseDatas[i]._showcaseID, caseDatas[i]._expetedTime_min);
                createShowCase.GetComponent<RectTransform>().FindChild("nameOfShowCase").GetComponent<Text>().text = caseDatas[i]._showcaseName;
                createShowCase.transform.SetParent(showCaseContentRect, false);
                showCaseButtonList.Add(createShowCase);
                StoreShowCaseButtons(createShowCase);
            }
        }
    }

    void StoreShowCaseButtons(GameObject obj)
    {
        storedShowCase.Add(obj);
    }

    void DeleteShowCase()
    {
        foreach (RectTransform child in showCaseContentRect)
        {
            if (child.name == "Prefab_ShowCase(Clone)")
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        storedShowCase.Clear();
    }

    public void anyButton()
    {
        Debug.Log("play scene");
    }

    public void AddShowCaseClicked()
    {
        isLocalCaseDone = true;
        gameObject.SetActive(false);
        isFileTransferClicked = true;
    }

    public void DeleteLocalShowcase(string deleteID)
    {
        _setManager.BShowcaseMgr.DeleteShowcase(deleteID);
        deleteCount++;
        Start();
    }

     public void EditShowCase(string _title, int _sizeOfRoom, int _audience, string _localPath, string _id, int _time)
    {
        customView.GetComponent<CustomizeView>().SetPPTID(_id);
        customView.GetComponent<CustomizeView>().SetCustomValueFromLocalView(_title, _sizeOfRoom, _audience, _localPath, _time);
        isCustomizeButtonClicked = true;
       
    }
 
}
