using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[RequireComponent(typeof(MeshRenderer))]
public class SlidesPlayer : MonoBehaviour
{
    public bool pptKaraoke;
    public float blendInterval = 1f;

    private List<Texture2D> _slides;
    private bool _isPlaying;
    private bool _isBlending;
    private int _currIdx;

    private Material _mat
    {
        get
        {
            Material mat = GetComponent<MeshRenderer>().material;
            if (mat != null)
                return mat;
            else
            {
                Material mat_new = new Material(Shader.Find("VR_Rehearsal_app/sh_Slides_Blend"));
                mat_new.name = "mat_Slides_Blend";
                GetComponent<MeshRenderer>().material = mat_new;
                return mat_new;
            }
        }
    }

    private void Start()
    {
        Stop();
        
        if (!pptKaraoke)
        {

            LoadSlidesFromDisk(PresentationData.in_SlidePath);
            /*Shiba
            if (GlobalManager.downloadManager != null)
            {
                List<string> slidesNames = GlobalManager.downloadManager.ExportExistedName();
                if (slidesNames.Count > 0)
                    LoadSlidesFromDisk(slidesNames[slidesNames.Count - 1]);
                else LoadSlides(new List<Texture2D>(Resources.LoadAll<Texture2D>("DefaultSlides")));
            }
            else
                LoadSlides(new List<Texture2D>(Resources.LoadAll<Texture2D>("DefaultSlides")));

            */
        }
        else
        {
            LoadSlides(new List<Texture2D>(Resources.LoadAll<Texture2D>("DefaultSlides")));
        }
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
        _mat.SetTexture("_CurrTex", Texture2D.blackTexture);
        _mat.SetTexture("_NextTex", Texture2D.blackTexture);
        _mat.SetFloat("_Blend", 0f);
    }

    public void NextSlide()
    {
        if (!_isPlaying || _isBlending || _currIdx >= _slides.Count - 1)
            return;
        _currIdx++;
        _mat.SetTexture("_NextTex", _slides[_currIdx]);
        StartCoroutine(Blend_CR());
    }

    public void PrevSlide()
    {
        if (!_isPlaying || _isBlending || _currIdx <= 0)
            return;
        _currIdx--;
        _mat.SetTexture("_NextTex", _slides[_currIdx]);
        StartCoroutine(Blend_CR());
    }

    private IEnumerator Blend_CR()
    {
        _isBlending = true;

        float deltaTime = 0f;
        while (deltaTime < blendInterval)
        {
            _mat.SetFloat("_Blend", Mathf.Clamp01(deltaTime / blendInterval));
            deltaTime += Time.deltaTime;
            yield return null;
        }
       
        _mat.SetTexture("_CurrTex", _mat.GetTexture("_NextTex"));
        _mat.SetFloat("_Blend", 0f);

        _isBlending = false;
    }

   
}
