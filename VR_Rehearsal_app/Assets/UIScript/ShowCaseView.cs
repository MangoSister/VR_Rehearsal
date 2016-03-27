using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections.Generic;

public class ShowCaseView : MonoBehaviour {
    //Initialize screen size

    //Initialize Dropbox api
    private bhClowdDriveAPI _bDriveAPI;
    private bShowcaseManager _bShowcaseMgr;
    public static bool isShowCaseDone;

    private List<GameObject> CreatedButton = new List<GameObject>();
    private bool isReseting = false;
    private GameObject selectedButton;
    private bool isCopy = false;

    public List<GameObject> storedButton = new List<GameObject>();
    public List<GameObject> storedShocase = new List<GameObject>();
   // public RectTransform RootRect;
    public GameObject CreateInstance;

    public bool isDropBoxStart = false;

    void Start () {
        GetComponent<RectTransform>().SetAsLastSibling();
        isShowCaseDone = false;

       
            _bDriveAPI = new bDropboxAPI();
            _bDriveAPI.StartAuthentication(delegate ()
            {
                _bDriveAPI.GetFileListFromPath("/", CreatePanels);
            });
        _bShowcaseMgr = new bShowcaseManager();
        _bShowcaseMgr.Start();
    }
	
	// Update is called once per frame
	void Update () {
        if(_bDriveAPI != null)
        {
            _bDriveAPI.Update();
        }
       
    }
    public void CreatePanels(string fileList)
    {
        var parseResult = JSON.Parse(fileList);
        //GridLayoutGroup gLayout = canvasScroll.GetComponent<GridLayoutGroup>();
        //float cellSize = gLayout.cellSize.y;
        //float span = gLayout.spacing.y;
        //float totalSizeofRect = (cellSize - span) * parseResult["entries"].Count;
        //RootRect.offsetMin = new Vector2(RootRect.offsetMin.x, -1 * (totalSizeofRect / 2));
        //    // RootRect.offsetMax = new Vector2(RootRect.offsetMin.x, -10);
        //RootRect.offsetMax = new Vector2(RootRect.offsetMin.x, 0);
        for (int index = 0; index < parseResult["entries"].Count; index++)
        {
            GameObject createInstance = Instantiate(CreateInstance) as GameObject;

            createInstance.GetComponent<ButtonType>().buttonName = parseResult["entries"][index]["name"];
            createInstance.GetComponent<ButtonType>().buttonType = parseResult["entries"][index][".tag"];
            createInstance.GetComponentInChildren<Text>().text = parseResult["entries"][index]["name"];
            Debug.Log(createInstance);
          //  createInstance.transform.SetParent(RootRect, false);

            CreatedButton.Add(createInstance);
           // StoreAllButtonStatus(createInstance);
        }

        _bDriveAPI.JobDone();
        //isButtonSelected = false;
    }
    public void anyButton()
    {

        _bDriveAPI = new bDropboxAPI();
        _bDriveAPI.StartAuthentication(delegate () {
            _bDriveAPI.GetFileListFromPath("/", CreatePanels);
        });
    }
    public void AddShowCaseClicked()
    {
        gameObject.SetActive(false);

    }



}
