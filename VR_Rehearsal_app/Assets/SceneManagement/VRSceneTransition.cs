using UnityEngine;
using System.Collections;

public class VRSceneTransition : GlobalBehaviorBase
{
    public void Fade(bool fadeIn, float length)
    {
        ScreenFader[] faders = FindObjectsOfType<ScreenFader>();
        foreach (ScreenFader fader in faders)
            fader.Fade(fadeIn, length);
    }
}
