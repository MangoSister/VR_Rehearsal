using UnityEngine;
using System.Collections;
using System;

public sealed class ScreenFader : ImageEffectBase
{
    public Color fadeColor = Color.black;

    [Range(0.0f, 1.0f)]
    public float fadeIntensity = 0f;

    private bool _isFade;

    protected override void Start()
    {
        shader = Shader.Find("VR_Rehearsal_app/sh_ScreenFade");
        _isFade = false;
        base.Start();
    }

    protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (shader != null)
        {
            material.SetColor("_FadeColor", fadeColor);
            material.SetFloat("_Intensity", fadeIntensity);
            Graphics.Blit(src, dest, material);
        }
        else Graphics.Blit(src, dest);
    }

    public void Fade(bool fadeIn, float length)
    {
        if (Camera.main != null)
            transform.parent = Camera.main.transform;
        if (!_isFade)
            StartCoroutine(Fade_CR(fadeIn, length));
    }

    private IEnumerator Fade_CR(bool fadeIn, float length)
    {
        _isFade = true;
        float initIntensity = fadeIn ? 1.0f : 0.0f;
        float time = 0.0f;
        while (time < length)
        {
            fadeIntensity = Mathf.Lerp(initIntensity, 1.0f - initIntensity, Mathf.Clamp01(time / length));
            time += Time.deltaTime;
            yield return null;
        }
        fadeIntensity = 1.0f - initIntensity;
        _isFade = false;
    }
}
