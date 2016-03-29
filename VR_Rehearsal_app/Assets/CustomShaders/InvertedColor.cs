using UnityEngine;
using System.Collections;
using System;

public sealed class InvertedColor : ImageEffectBase
{
    protected override void Start()
    {
        shader = Shader.Find("VR_Rehearsal_app/sh_InvertedColor");
        base.Start();
    }

    protected override void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (shader != null)
        {
            Graphics.Blit(src, dest, material);
        }
        else Graphics.Blit(src, dest);
    }
}
