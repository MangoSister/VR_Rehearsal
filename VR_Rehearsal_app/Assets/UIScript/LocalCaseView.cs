using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public class LocalCaseView : MonoBehaviour {

    public static bool isLocalCaseDone;
    private SetupManager _setManager;


    public List<GameObject> storedShowCase = new List<GameObject>();
    private List<GameObject> showCaseButtonList = new List<GameObject>();

    private Vector2 _initialScrollContentSize;
    public GameObject canvasScroll;
    public RectTransform showCaseContentRect;
    float originalRect;

    public GameObject showCasePrefab;
    public GameObject contentRect;
    // Use this for initialization
    void Start () {
        isLocalCaseDone = false;
        CheckLocalPPT();
        GetComponent<RectTransform>().SetAsLastSibling();
        originalRect = showCaseContentRect.offsetMin.y;
        GetComponent<RectTransform>().SetAsLastSibling();
        _initialScrollContentSize = new Vector2(showCaseContentRect.rect.height, showCaseContentRect.rect.width);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetSetupManager(SetupManager mg)
    {
        _setManager = mg;

    }

    void CheckLocalPPT()
    {
        DeleteShowCase();
        bShowcaseManager.showcase_Data[] caseDatas = _setManager.BShowcaseMgr.GetAllShowcases();
        GridLayoutGroup gLayout_showCase = contentRect.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout_showCase.cellSize.y;
        float span = gLayout_showCase.spacing.y;
        float totalSizeofRect = (cellSize - span) * caseDatas.Length;
        for (int i = 0; i < caseDatas.Length; ++i)
        {
            Debug.Log(i + ": " + caseDatas[i]._showcaseID + "," + caseDatas[i]._showcaseName);
            GameObject createShowCase = Instantiate(showCasePrefab) as GameObject;
            createShowCase.GetComponent<ShowCaseButton>().SetData(caseDatas[i]._showcaseName, caseDatas[i]._mapIdx, caseDatas[i]._percentageOfAudience, caseDatas[i]._pptFolderPath, caseDatas[i]._showcaseID, 5);
            createShowCase.GetComponent<RectTransform>().FindChild("nameOfShowCase").GetComponent<Text>().text = caseDatas[i]._showcaseName;
            showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, -1 * (totalSizeofRect / 2));
            createShowCase.transform.SetParent(showCaseContentRect, false);
            showCaseButtonList.Add(createShowCase);
            StoreShowCaseButtons(createShowCase);
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
    }
}
