using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrepHouseKeeper : MonoBehaviour
{
    public InputField urlField;
    public InputField dbNumField;
    public InputField commentField;
    public Button downloadBtn;
    public GameObject uiManager;
    public Text findBtnTxt;

    public Text transitionTxt;

    private DownloadManager downloadManager
    { get { return SceneManager.downloadManager; } }

    public void StartDownload()
    {
        if (downloadManager == null)
        {
            Debug.Log("Nothing downloaded");
            return;
        }
        else {
            downloadManager.LaunchDownload(urlField.text, dbNumField.text);
            downloadBtn.interactable = false;
            Debug.Log("Something downloading");
        }
    }

    public void FinishDownload(bool success, string targetLoc)
    {
        if (success)
        {
            urlField.gameObject.SetActive(false);
            dbNumField.gameObject.SetActive(false);
            downloadBtn.gameObject.SetActive(false);
            transitionTxt.gameObject.SetActive(true);
            //StartCoroutine(Transition_CR());
            uiManager.GetComponent<UIManager>().SetPowerPointData(commentField.text);
            uiManager.GetComponent<UIManager>().ShowListPanel();
        }
        else
        {
            transitionTxt.gameObject.SetActive(true);
#if UNITY_EDITOR
            Debug.Log("Download failed");
#endif
            downloadBtn.interactable = true;
        }
    }

    private void Start()
    {
        transitionTxt.gameObject.SetActive(false);
        urlField.gameObject.SetActive(true);
        dbNumField.gameObject.SetActive(true);
        downloadBtn.gameObject.SetActive(true);

        if (downloadManager != null)
        {
            downloadManager.OnPostDownload.AddListener(FinishDownload);
        }

    }

    private IEnumerator Transition_CR()
    {
        float time = 0.0f;
        while (time < 6.0f)
        {
            transitionTxt.text = string.Format("Waiting: {0} sec",
                Mathf.FloorToInt(6.0f - time));
            time += Time.deltaTime;
            yield return null;
        }

        if (SceneManager.screenTransition != null)
            SceneManager.screenTransition.Fade(false, 1.0f);

        yield return new WaitForSeconds(1.0f);

        SceneManager.LaunchPresentationScene(new PresentationInitParam("sc_present_0"));
    }

    // PPT button in List Panel
    // At this time, change to present_0 scene
    public void OnPPTSlideClick()
    {
        SceneManager.LaunchPresentationScene(new PresentationInitParam("sc_present_0"));
    }
 }
