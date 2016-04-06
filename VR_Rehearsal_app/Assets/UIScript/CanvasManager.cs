using UnityEngine;
using System.Collections;

public class CanvasManager : MonoBehaviour {

    public GameObject logo;
    public GameObject localShowCase;
    public GameObject fileTranser;
    public GameObject navigation;
    public GameObject loading;
    public GameObject customize;
    public GameObject calibration;
    public GameObject rotation;
	public GameObject showCaseButton;
	private SetupManager _setupManager;

   
	void Awake () {

        // Makes the status bar and navigation bar visible over the content (different content resize method) 
        //ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.VisibleOverContent;
        localShowCase.SetActive(false);
        fileTranser.SetActive(false);
        navigation.SetActive(false);
        loading.SetActive(false);
        customize.SetActive(false);
        _setupManager = new SetupManager();
        localShowCase.GetComponent<LocalCaseView>().SetSetupManager(_setupManager);
        navigation.GetComponent<NavigationView>().SetSetupManager(_setupManager);
        customize.GetComponent<CustomizeView>().SetSetupManager(_setupManager);
        rotation.GetComponent<RotationView>().SetSetupManager(_setupManager);
        calibration.SetActive(false);
        rotation.SetActive(false);

    }


    void Update () {
        /*
        if (LogoView.isLogoSceneDone)
        {
            localShowCase.SetActive(true);
            LogoView.isLogoSceneDone = false;
            fileTranser.SetActive(false);
        }
        if (LocalCaseView.isLocalCaseDone)
        {
            fileTranser.SetActive(true);
            LocalCaseView.isLocalCaseDone = false;
        }
        if (FileTransferView.isFileTransferViewDone)
        {
            int transferType = fileTranser.GetComponent<FileTransferView>().transferNumber;
            navigation.SetActive(true);
            Debug.Log(transferType);
            navigation.GetComponent<NavigationView>().SetupCloud(transferType);
            FileTransferView.isFileTransferViewDone = false;
        }
        if (NavigationView.isNavigationDone)
        {
            NavigationView.isNavigationDone = false;
            customize.SetActive(true);
        }
        if (CustomizeView.isCustomizeDone)
        {
            CustomizeView.isCustomizeDone = false;
            calibration.SetActive(true);
        }
        if (CalibrationView.isCalibrationDone)
        {
            CalibrationView.isCalibrationDone = false;
            rotation.SetActive(true);
            rotation.GetComponent<RotationView>().SetRotation(true);
        }
        */
        ShowLocasShowView();
        ShowFileTransferView();
        ShowNavigationView();
        ShowCustomView();
        ShowCalibrationView();
        ShowRotationView();
        NavigationBetweenView();
        ShowCustomViewFromLocalCaseView();
        ShowCustomViewToLocalView();


    }
    public void ShowLocasShowView()
    {
        if (LogoView.isLogoSceneDone)
        {
            localShowCase.SetActive(true);
            LogoView.isLogoSceneDone = false;
            fileTranser.SetActive(false);
        }
    }

    public void ShowFileTransferView()
    {
        if (LocalCaseView.isLocalCaseDone && localShowCase.GetComponent<LocalCaseView>().isFileTransferClicked==true)
        {
            fileTranser.SetActive(true);
            LocalCaseView.isLocalCaseDone = false;
            localShowCase.GetComponent<LocalCaseView>().isFileTransferClicked = false;
        }
    }
    public void ShowNavigationView()
    {
        if (FileTransferView.isFileTransferViewDone)
        {
            int transferType = fileTranser.GetComponent<FileTransferView>().transferNumber;
            navigation.SetActive(true);
            Debug.Log(transferType);
            navigation.GetComponent<NavigationView>().SetupCloud(transferType);
            FileTransferView.isFileTransferViewDone = false;
        }
    }
    public void ShowCustomView()
    {
         if (NavigationView.isNavigationDone)
        {
            NavigationView.isNavigationDone = false;
            customize.SetActive(true);
            loading.SetActive(false);
        }
    }
   public void ShowCustomViewFromLocalCaseView()
    {
        if (LocalCaseView.isCustomizeButtonClicked)
        {
            LocalCaseView.isCustomizeButtonClicked = false;
            localShowCase.SetActive(false);
            customize.SetActive(true);
        }
    }
    public void ShowCustomViewToLocalView()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (customize.GetComponent<CustomizeView>().isCustomizeDoneFromLocal == true)
            {
                customize.SetActive(false);
                localShowCase.SetActive(true);
                customize.GetComponent<CustomizeView>().isCustomizeDoneFromLocal = false;
            }
        }
    }
    public void ShowCalibrationView()
    {
        if (CustomizeView.isCustomizeDone)
        {
            CustomizeView.isCustomizeDone = false;
            calibration.SetActive(true);
        }
    }
    public void ShowRotationView()
    {
		
        if (CalibrationView.isCalibrationDone)
        {
            CalibrationView.isCalibrationDone = false;
            rotation.SetActive(true);
            rotation.GetComponent<RotationView>().SetRotation(true);
        }
    }
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
    void NavigationBetweenView()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (fileTranser.activeSelf)
            {
                fileTranser.SetActive(false);
                FileTransferView.isFileTransferViewDone = false;
                localShowCase.SetActive(true);
            }
            if (navigation.activeSelf)
            {
                string str = navigation.GetComponent<NavigationView>().RecentPath();
                Debug.Log(str);
                if (str == "/" || str == ""){
                    navigation.SetActive(false);
                    NavigationView.isNavigationDone = false;
                    navigation.GetComponent<NavigationView>().DeletePanels(true, "ok");
                    fileTranser.SetActive(true);
                }
            }
        }
    }
}
