﻿using UnityEngine;
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
        Debug.Log(originalRect);
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
        if (caseDatas == null)
        {
            return;
        }
        GridLayoutGroup gLayout_showCase = contentRect.GetComponent<GridLayoutGroup>();
        float cellSize = gLayout_showCase.cellSize.y;
        float span = gLayout_showCase.spacing.y;
        float totalSizeofRect = (cellSize - (span/2)) * caseDatas.Length;
       
        if (caseDatas!=null)
        {
            if (caseDatas.Length < 7)
            {
                showCaseContentRect.offsetMax = new Vector2(showCaseContentRect.offsetMin.x, -12f);
                showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, originalRect);
            }
            else
            {
                gLayout_showCase.padding.top = -68;
                showCaseContentRect.offsetMax = new Vector2(showCaseContentRect.offsetMin.x, -12f);
                //showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, (-1 * totalSizeofRect / 2) + ((span + caseDatas.Length) / 3));
                //showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, (originalRect +((cellSize - span) * (caseDatas.Length - 5))));
               showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, -(originalRect+(totalSizeofRect/2)));
            }

            for (int i = 0; i < caseDatas.Length; ++i)
            {
                GameObject createShowCase = Instantiate(showCasePrefab) as GameObject;

                //  createShowCase.GetComponentInChildren<ShowCaseButton>().SetData(caseDatas[i]._showcaseName, caseDatas[i]._mapIdx, caseDatas[i]._percentageOfAudience, caseDatas[i]._pptFolderPath, caseDatas[i]._showcaseID, caseDatas[i]._expetedTime_min);
                createShowCase.GetComponent<ShowCaseButton>().SetData(caseDatas[i]._showcaseName, caseDatas[i]._mapIdx, caseDatas[i]._percentageOfAudience, caseDatas[i]._pptFolderPath, caseDatas[i]._showcaseID, caseDatas[i]._expetedTime_min);

                createShowCase.GetComponent<RectTransform>().FindChild("nameOfShowCase").GetComponent<Text>().text = caseDatas[i]._showcaseName;
                //showCaseContentRect.offsetMin = new Vector2(showCaseContentRect.offsetMin.x, -1 * (totalSizeofRect / 2));
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
    }

    public void DeleteLocalShowcase(string deleteID)
    {
        Debug.Log("call Delete function");
        _setManager.BShowcaseMgr.DeleteShowcase(deleteID);
     
    }
}
