using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections.Generic;

public class ShowCaseView : MonoBehaviour {

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
            _bDriveAPI.StartAuthentication(delegate (bool res)
            {
                if (res)
                {
                    _bDriveAPI.GetFileListFromPath("/", CreatePanels);
                }
            });
        _bShowcaseMgr = new bShowcaseManager();
    }

	void Update () {
        if(_bDriveAPI != null)
        {
            _bDriveAPI.Update();
        }
       
    }
    public void CreatePanels(string fileList)
    {
        var parseResult = JSON.Parse(fileList);
        for (int index = 0; index < parseResult["entries"].Count; index++)
        {
            GameObject createInstance = Instantiate(CreateInstance) as GameObject;

            createInstance.GetComponent<ButtonType>().buttonName = parseResult["entries"][index]["name"];
            createInstance.GetComponent<ButtonType>().buttonType = parseResult["entries"][index][".tag"];
            createInstance.GetComponentInChildren<Text>().text = parseResult["entries"][index]["name"];
            CreatedButton.Add(createInstance);
        }

        _bDriveAPI.JobDone();
    }
    public void anyButton()
    {

        _bDriveAPI = new bDropboxAPI();
        _bDriveAPI.StartAuthentication(delegate (bool res) {
            if (res)
            {
                _bDriveAPI.GetFileListFromPath("/", CreatePanels);
            }
        });
    }
    public void AddShowCaseClicked()
    {
        gameObject.SetActive(false);

    }



}
