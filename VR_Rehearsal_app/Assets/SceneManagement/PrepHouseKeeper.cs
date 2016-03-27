﻿using System;
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

   // private DownloadManager downloadManager
    //{ get { return GlobalManager.downloadManager; } }
	/*
    public void StartDownload()
    {
      //  if (downloadManager == null)
        {
#if UNITY_EDITOR
            Debug.Log("Nothing downloaded");
#endif
            return;
        }
        else {
           // downloadManager.LaunchDownload(urlField.text, dbNumField.text);
            downloadBtn.interactable = false;
#if UNITY_EDITOR
            Debug.Log("Something downloading");
#endif
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

            uiManager.GetComponent<UIManager>().ShowCasePanel();

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
*/
    private void Start()
    {
       transitionTxt.gameObject.SetActive(false);
        urlField.gameObject.SetActive(true);
        dbNumField.gameObject.SetActive(true);
        downloadBtn.gameObject.SetActive(true);

		/* Obsolete 3/25/2016 by Byunghwan Lee
        if (downloadManager != null)
        {
            downloadManager.OnPostDownload.AddListener(FinishDownload);
        }*/

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

        if (GlobalManager.screenTransition != null)
            GlobalManager.screenTransition.Fade(false, 1.0f);

        yield return new WaitForSeconds(1.0f);

        GlobalManager.EnterPresentation();
    }

    // PPT button in List Panel
    // At this time, change to present_0 scene
    public void NextScene()
    {
        GlobalManager.EnterPresentation();
        //SceneManager.LaunchPresentationScene(new PresentationInitParam("sc_rotation"));
    }
 }
