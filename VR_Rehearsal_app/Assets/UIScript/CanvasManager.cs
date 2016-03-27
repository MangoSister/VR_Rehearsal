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
    private SetupManager _setupManager;
   
	void Awake () {
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
        
        if (LogoView.isLogoSceneDone)
        {
            ShowLocalShowCaseView();
            //localShowCase.SetActive(true);
            //  LogoView.isLogoSceneDone = false;
            //  fileTranser.SetActive(false);
        }
        if (LocalCaseView.isLocalCaseDone)
        {
            ShowFileTransferView();
          //  fileTranser.SetActive(true);
          //  LocalCaseView.isLocalCaseDone = false;
        }
        if (FileTransferView.isFileTransferViewDone)
        {
            ShowNavigationView();
          //  int transferType = fileTranser.GetComponent<FileTransferView>().transferNumber;
          //    navigation.SetActive(true);
          //    Debug.Log(transferType);
          //    navigation.GetComponent<NavigationView>().SetupCloud(transferType);
          //    FileTransferView.isFileTransferViewDone = false;
        }
        if (NavigationView.isNavigationDone)
        {
            ShowNavigationView();
         //   NavigationView.isNavigationDone = false;
         //   customize.SetActive(true);
        }
        if (CustomizeView.isCustomizeDone)
        {
            ShowCustomizeView();
          //  CustomizeView.isCustomizeDone = false;
          //  calibration.SetActive(true);
        }
        if (CalibrationView.isCalibrationDone)
        {
            ShowRotationView();
          //  CalibrationView.isCalibrationDone = false;
          //   rotation.SetActive(true);
          //   rotation.GetComponent<RotationView>().SetRotation(true);
        }
        
        /*
        ShowLocalShowCaseView();
        ShowFileTransferView();
        ShowNavigationView();
        ShowCustomizeView();
        ShowRotationView();
        NavigationBetweenView();
        */
    }
    public void ShowLocalShowCaseView()
    {
            localShowCase.SetActive(true);
            LogoView.isLogoSceneDone = false;
            fileTranser.SetActive(false);
    }
    public void ShowFileTransferView()
    {
            fileTranser.SetActive(true);
            LocalCaseView.isLocalCaseDone = false;
    }

    public void ShowNavigationView()
    {
        int transferType = fileTranser.GetComponent<FileTransferView>().transferNumber;
        navigation.SetActive(true);
        Debug.Log(transferType);
        navigation.GetComponent<NavigationView>().SetupCloud(transferType);

    }
    public void ShowCustomizeView()
    {
        NavigationView.isNavigationDone = false;
        customize.SetActive(true);
    }
    public void ShowRotationView()
    {
        CalibrationView.isCalibrationDone = false;
        rotation.SetActive(true);
        rotation.GetComponent<RotationView>().SetRotation(true);

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
                if (str == "/"){
                    navigation.SetActive(false);
                    NavigationView.isNavigationDone = false;
                    navigation.GetComponent<NavigationView>().DeletePanels(true, "ok");
                    fileTranser.SetActive(true);
                }
            }
        }
    }
}
