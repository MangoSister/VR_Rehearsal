using UnityEngine;
using System.Collections;

public class CanvasManager : MonoBehaviour {

    public LogoView logo;
    public LocalCaseView localShowCase;
    public FileTransferView fileTranser;
    public NavigationView navigation;
    public GameObject loading;
    public CustomizeView customize;
    public CalibrationView calibration;
    public RotationView rotation;
	public GameObject showCaseButton;

    static public bool againTrigger = false;
    static public bool finishTrigger = false;

	private SetupManager _setupManager;
    bShowcaseManager.showcase_Data customData;
    private string _showCaseName;
    private int _sizeOfRoom;
    private int _numberOfAudience;
    private string _localPath;
    private string _id;
    private int _expectedTime;
    public bool isFromCustom;

    public void SetData(string showCanseName, int sizeOfRoom, int numberOfAudience, string localPath, string id, int time)
    {
        _showCaseName = showCanseName;
        _sizeOfRoom = sizeOfRoom;
        _numberOfAudience = numberOfAudience;
        _localPath = localPath;
        _id = id;
        _expectedTime = time;
    }
    public string GetShowCaseName()
    {
        return _showCaseName;
    }
    public int GetRoom()
    {
        return _sizeOfRoom;
    }
    public int GetAudience()
    {
        return _numberOfAudience;
    }
    public string GetLocalPath()
    {
        return _localPath;
    }
    public string GetPPTID()
    {
        return _id;
    }
    public int GetTime()
    {
        return _expectedTime;
    }
    public void SetPPTID(string str)
    {
        _id = str;
        GetDataByID(_id);
    }
    public void GetDataByID(string pptID)
    {
        bShowcaseManager.showcase_Data? tempShocase = _setupManager.BShowcaseMgr.GetSignleShowcase(_id);
        if (tempShocase != null) {

            SetData(tempShocase.Value._showcaseName, (int)tempShocase.Value._mapIdx, (int)tempShocase.Value._percentageOfAudience, tempShocase.Value._pptFolderPath, tempShocase.Value._showcaseID, (int)tempShocase.Value._expetedTime_min);      
        }
        
    }
    void Awake () {
        localShowCase.gameObject.SetActive(false);
        fileTranser.gameObject.SetActive(false);
        navigation.gameObject.SetActive(false);
        loading.SetActive(false);
        customize.gameObject.SetActive(false);
        _setupManager = new SetupManager();
        localShowCase.GetComponent<LocalCaseView>().SetSetupManager(_setupManager);
        navigation.GetComponent<NavigationView>().SetSetupManager(_setupManager);
        customize.GetComponent<CustomizeView>().SetSetupManager(_setupManager);
        rotation.GetComponent<RotationView>().SetSetupManager(_setupManager);
        calibration.gameObject.SetActive(false);
        rotation.gameObject.SetActive(false);
    }
   public void SetisFromCustom(bool res)
    {
        isFromCustom = res;
    }
    public bool GetisFromCustom()
    {
        return isFromCustom;
        
    }
   public void PlayAgainButtonClicked()
    {
        if(againTrigger == true)
        {
            ShowRotationView();
        }
    }
    void Update () {

        ShowLocalFromReview();
        ShowFileTransferView();
        ShowNavigationView();
        ShowCustomView();
        ShowCalibrationView();
        ShowRotationView();
        NavigationBetweenView();
        ShowCustomViewFromLocalCaseView();
        ShowCustomViewToLocalView();

    }
    public void ShowLocalFromReview()
    {
        if (LogoView.isLogoSceneDone)
        {
            localShowCase.gameObject.SetActive(true);
            LogoView.isLogoSceneDone = false;
            fileTranser.gameObject.SetActive(false);
        }
        else if(finishTrigger == true)
        {
            localShowCase.gameObject.SetActive(true);
            LogoView.isLogoSceneDone = false;
            fileTranser.gameObject.SetActive(false);
            //     customize.gameObject.SetActive(false);
            finishTrigger = false;
        }
        else if (againTrigger ==true)
        {
            localShowCase.gameObject.SetActive(false);
        }
    }

    public void ShowFileTransferView()
    {
        if (LocalCaseView.isLocalCaseDone && localShowCase.GetComponent<LocalCaseView>().isFileTransferClicked==true)
        {
            fileTranser.gameObject.SetActive(true);
            LocalCaseView.isLocalCaseDone = false;
            localShowCase.GetComponent<LocalCaseView>().isFileTransferClicked = false;
        }
    }
    public void ShowNavigationView()
    {
        if (FileTransferView.isFileTransferViewDone)
        {
            int transferType = fileTranser.GetComponent<FileTransferView>().transferNumber;
            navigation.gameObject.SetActive(true);
            navigation.GetComponent<NavigationView>().SetupCloud(transferType);
            FileTransferView.isFileTransferViewDone = false;
        }
    }
    public void ShowCustomView()
    {
         if (NavigationView.isNavigationDone)
        {
            NavigationView.isNavigationDone = false;
            customize.gameObject.SetActive(true);
            loading.SetActive(false);
        }
    }
   public void ShowCustomViewFromLocalCaseView()
    {
        if (LocalCaseView.isCustomizeButtonClicked)
        {
            LocalCaseView.isCustomizeButtonClicked = false;
            localShowCase.gameObject.SetActive(false);
            customize.gameObject.SetActive(true);
        }
    }
    public void ShowCustomViewToLocalView()
    {

        if (Input.GetKey(KeyCode.Escape) && customize.GetComponent<CustomizeView>().isCustomizeDoneFromLocal == true)
        {
             customize.gameObject.SetActive(false);
             localShowCase.gameObject.SetActive(true);
             customize.GetComponent<CustomizeView>().isCustomizeDoneFromLocal = false;
        }
    }
    public void ShowCalibrationView()
    {
        if (CustomizeView.isCustomizeDone)
        {
            CustomizeView.isCustomizeDone = false;
            calibration.gameObject.SetActive(true);
        }
       
    }
    public void ShowRotationView()
    {
		
        //if (CalibrationView.isCalibrationDone)
        if (CalibrationControlNew.isCalibrationDone)
        {
            //CalibrationView.isCalibrationDone = false;
            CalibrationControlNew.isCalibrationDone = false;
            rotation.gameObject.SetActive(true);
            rotation.GetComponent<RotationView>().SetRotation(true);
        }
        else if(againTrigger == true)
        {

            rotation.gameObject.SetActive(true);
            localShowCase.gameObject.SetActive(false);
            rotation.GetComponent<RotationView>().SetRotation(true);

        }
    }
    public void DirectShowCalibrationView()
    {
        foreach (GameObject temp in localShowCase.GetComponent<LocalCaseView>().storedShowCase)
        {
            if (temp.GetComponent<ShowCaseButton>().isShowcaseButtonClicked == true)
            {
                localShowCase.gameObject.SetActive(false);
                calibration.gameObject.SetActive(true);
            }

        }
    }
    /*
	public void DirectShowRotationView(){
        foreach(GameObject temp in localShowCase.GetComponent<LocalCaseView>().storedShowCase)
        {
            if(temp.GetComponent<ShowCaseButton>().isShowcaseButtonClicked == true)
            {
                localShowCase.SetActive(false);
                rotation.SetActive(true);
                rotation.GetComponent<RotationView>().SetRotation(true);
            }

        }
	}
    */
      void NavigationBetweenView()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NavigationView nv = navigation.GetComponent<NavigationView>();
            if (fileTranser.gameObject.activeSelf)
            {
                fileTranser.gameObject.SetActive(false);
                FileTransferView.isFileTransferViewDone = false;
                localShowCase.gameObject.SetActive(true);
            }
            if (navigation.gameObject.activeSelf)
            {
				//1. Auth Failed Case
				if (!nv.GetAuthenticationStatus ()) {
					navigation.gameObject.SetActive(false);
					NavigationView.isNavigationDone = false;
					nv.DeletePanels(true, "ok");
					nv.Initialize ();
					fileTranser.gameObject.SetActive(true);
					return;
				}
					
				//2. Back button Keep pushing case
                string str = nv.RecentPath();
                Debug.Log(str);
				if (str == "null") {
					return;
				}

                if (str == "/" || str == ""){
                    navigation.gameObject.SetActive(false);
                    NavigationView.isNavigationDone = false;
                    nv.DeletePanels(true, "ok");
					nv.Initialize ();
                    fileTranser.gameObject.SetActive(true);
                }

            }
        }
    }
}
