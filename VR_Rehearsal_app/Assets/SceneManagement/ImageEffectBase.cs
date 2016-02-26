using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public abstract class ImageEffectBase : MonoBehaviour
{
    public Shader shader;

    private Material _mat;
    public Material material
    {
        get
        {
            if (_mat == null)
            {
                _mat = new Material(shader);
                _mat.hideFlags = HideFlags.HideAndDontSave;
            }
            return _mat;
        }
    }

    // Use this for initialization
    protected virtual void Start()
    {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        if (!shader || !shader.isSupported)
            enabled = false;
    }

    protected abstract void OnRenderImage(RenderTexture src, RenderTexture dest);

    private void OnDisable()
    {
        if (_mat)
            DestroyImmediate(_mat);
    }
}
