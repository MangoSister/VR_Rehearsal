﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LogoView : MonoBehaviour {

   public static bool isLogoSceneDone;
    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        isLogoSceneDone = false;
        GetComponent<RectTransform>().SetAsLastSibling();
        if (!CanvasManager.finishTrigger)
        {
            StartCoroutine("ChangePanel");
        }
        
    }
    public IEnumerator ChangePanel()
    {
        yield return new WaitForSeconds(2.0f);
        isLogoSceneDone = true;
        this.gameObject.SetActive(false);
    }
}
	

