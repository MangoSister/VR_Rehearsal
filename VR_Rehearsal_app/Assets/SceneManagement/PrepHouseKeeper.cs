using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrepHouseKeeper : MonoBehaviour
{
    private void Start()
    {


    }

    private IEnumerator Transition_CR()
    {
        float time = 0.0f;
        while (time < 6.0f)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (GlobalManager.screenTransition != null)
            GlobalManager.screenTransition.Fade(false, 1.0f);

        yield return new WaitForSeconds(1.0f);

        GlobalManager.EnterPresentation();
    }

    public void NextScene()
    {
        GlobalManager.EnterPresentation();
    }
 }
