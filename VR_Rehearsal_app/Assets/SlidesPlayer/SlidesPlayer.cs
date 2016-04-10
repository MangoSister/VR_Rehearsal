/* SlidesPlayer.cs
 * Yang Zhou, last modified on March 25, 2016
 * The SlidesPlayer play a series of texture2d as slides, and provides blending transition
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SlidesPlayer : MonoBehaviour
{
    public List<MeshRenderer> displays;
    public float blendInterval = 1f;

    private List<Texture2D> _slides;
    private bool _isPlaying;
    private bool _isBlending;
    private int _currIdx;
    public int CurrIdx { get { return _currIdx; } }

    private Material _mat;

    private Material mat
    {
        get
        {
            if (_mat == null)
            {
                _mat = new Material(Shader.Find("VR_Rehearsal_app/sh_Slides_Blend"));
                _mat.name = "mat_Slides_Blend";
                foreach (MeshRenderer mr in displays)
                    mr.material = _mat;
            }
            return _mat;
        }
    }

    private void Awake()
    {
        Stop();

        if (!string.IsNullOrEmpty(PresentationData.in_SlidePath))
            LoadSlidesFromDisk(PresentationData.in_SlidePath);
        else
            LoadSlides(new List<Texture2D>(Resources.LoadAll<Texture2D>("DefaultSlides")));

    }

    public bool LoadSlides(List<Texture2D> slides)
    {
        if (_isPlaying)
            return false;
        else
        {
            UnLoadSlides();
            _slides = slides;
            return true;
        }

    }

    public bool LoadSlidesFromResources(string path)
    {
        if (_isPlaying)
            return false;
        else
        {
            UnLoadSlides();
            _slides = new List<Texture2D>(Resources.LoadAll<Texture2D>(path));
            return true;
        }

    }

    public bool LoadSlidesFromDisk(string path)
    {
        if (_isPlaying)
            return false;
        else
        {
            UnLoadSlides();
            //this multiple filter is not working. 
            //string[] imgNames = Directory.GetFiles(path, "*.png|*.jpg|*.bmp", SearchOption.TopDirectoryOnly);
            string[] imgNames_png = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
            string[] imgNames_jpg = Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] imgNames_bmp = Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly);

            
            string[] imgNames = new string[imgNames_png.Length + imgNames_jpg.Length + imgNames_bmp.Length];
            
            System.Array.Copy(imgNames_png, imgNames, imgNames_png.Length);
            System.Array.Copy(imgNames_jpg, 0, imgNames, imgNames_png.Length, imgNames_jpg.Length);
            System.Array.Copy(imgNames_bmp, 0, imgNames, imgNames_jpg.Length, imgNames_bmp.Length);
            
            _slides = new List<Texture2D>();
            foreach (string name in imgNames)
            {
                byte[] data = File.ReadAllBytes(name);
                _slides.Add(new Texture2D(1, 1));
                _slides[_slides.Count - 1].LoadImage(data);
            }
            return true;
        }
    }

    public bool UnLoadSlides()
    {
        if (_isPlaying || _slides == null)
            return false;

        //foreach (Texture2D page in _slides)
        //    Texture2D.Destroy(page);

        _slides = null;
        return true;
    }

    private void OnDestroy()
    {
        Stop();
        UnLoadSlides();
    }

    public void Play(int idx = 0)
    {
        if (_isPlaying || _slides == null || _slides.Count == 0 || idx < 0 || idx > _slides.Count)
            return;

        _isPlaying = true;
        _currIdx = -1;
        NextSlide();
    }

    public void Stop()
    {
        _isPlaying = false;
        _isBlending = false;
        mat.SetTexture("_CurrTex", Texture2D.blackTexture);
        mat.SetTexture("_NextTex", Texture2D.blackTexture);
        mat.SetFloat("_Blend", 0f);
    }

    public void NextSlide()
    {
        if (!_isPlaying || _isBlending || _currIdx >= _slides.Count - 1)
            return;
        _currIdx++;
        mat.SetTexture("_NextTex", _slides[_currIdx]);
        StartCoroutine(Blend_CR());
    }

    public void PrevSlide()
    {
        if (!_isPlaying || _isBlending || _currIdx <= 0)
            return;
        _currIdx--;
        mat.SetTexture("_NextTex", _slides[_currIdx]);
        StartCoroutine(Blend_CR());
    }

    private IEnumerator Blend_CR()
    {
        _isBlending = true;

        float deltaTime = 0f;
        while (deltaTime < blendInterval)
        {
            mat.SetFloat("_Blend", Mathf.Clamp01(deltaTime / blendInterval));
            deltaTime += Time.deltaTime;
            yield return null;
        }
       
        mat.SetTexture("_CurrTex", mat.GetTexture("_NextTex"));
        mat.SetFloat("_Blend", 0f);

        _isBlending = false;
    }

   
}
