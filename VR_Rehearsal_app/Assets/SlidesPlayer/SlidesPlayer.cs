/* SlidesPlayer.cs
 * Yang Zhou, last modified on March 25, 2016
 * The SlidesPlayer play a series of texture2d as slides, and provides blending transition
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SlidesPlayer : MonoBehaviour
{
    public List<MeshRenderer> displays;
    public float blendInterval = 1f;

    public List<Texture2D> _slides { get; private set; }
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

            //it seems like windows & android have different case-sensitivity here
#if !UNITY_EDITOR && UNITY_ANDROID
            //this multiple filter is not working. 
            //string[] imgNames = Directory.GetFiles(path, "*.png|*.jpg|*.bmp", SearchOption.TopDirectoryOnly);
            string[] imgNames_png = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
            string[] imgNames_jpg = Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] imgNames_bmp = Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly);
			string[] imgNames_PNG = Directory.GetFiles(path, "*.PNG", SearchOption.TopDirectoryOnly);
			string[] imgNames_JPG = Directory.GetFiles(path, "*.JPG", SearchOption.TopDirectoryOnly);
			string[] imgNames_BMP = Directory.GetFiles(path, "*.BMP", SearchOption.TopDirectoryOnly);

            
			string[] imgNames = new string[imgNames_png.Length + imgNames_jpg.Length + imgNames_bmp.Length + imgNames_PNG.Length + imgNames_JPG.Length + imgNames_BMP.Length];
            
            System.Array.Copy(imgNames_png, imgNames, imgNames_png.Length);
            System.Array.Copy(imgNames_jpg, 0, imgNames, imgNames_png.Length, imgNames_jpg.Length);
            System.Array.Copy(imgNames_bmp, 0, imgNames, imgNames_jpg.Length + imgNames_png.Length, imgNames_bmp.Length);
			System.Array.Copy(imgNames_PNG, 0, imgNames, imgNames_bmp.Length + imgNames_jpg.Length + imgNames_png.Length, imgNames_PNG.Length);
			System.Array.Copy(imgNames_JPG, 0, imgNames, imgNames_PNG.Length + imgNames_bmp.Length + imgNames_jpg.Length + imgNames_png.Length, imgNames_JPG.Length);
			System.Array.Copy(imgNames_BMP, 0, imgNames, imgNames_JPG.Length + imgNames_PNG.Length + imgNames_bmp.Length + imgNames_jpg.Length + imgNames_png.Length, imgNames_BMP.Length);
#else
            string[] imgNames_png = Directory.GetFiles(path, "*.png", SearchOption.TopDirectoryOnly);
            string[] imgNames_jpg = Directory.GetFiles(path, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] imgNames_bmp = Directory.GetFiles(path, "*.bmp", SearchOption.TopDirectoryOnly);

            string[] imgNames = new string[imgNames_png.Length + imgNames_jpg.Length + imgNames_bmp.Length];

            System.Array.Copy(imgNames_png, imgNames, imgNames_png.Length);
            System.Array.Copy(imgNames_jpg, 0, imgNames, imgNames_png.Length, imgNames_jpg.Length);
            System.Array.Copy(imgNames_bmp, 0, imgNames, imgNames_jpg.Length + imgNames_png.Length, imgNames_bmp.Length);
#endif
           
			//****slides need a sorting *****-comment by Byunghwan Lee May 2 2016

			//Classification process for sorting pages
			List<string> fileList_pptFormat = new List<string>();
			List<string> fileList_unknownFormat = new List<string>();

			for(int i = 0; i < imgNames.Length; ++i){
				string tempRes = imgNames[i].Replace( path + "\\", "");
				if (tempRes.Contains ("Slide")) {
					fileList_pptFormat.Add (imgNames [i]);
				} else {
					fileList_unknownFormat.Add (imgNames [i]);
				}
			}
				
			//when exported Properly by Powerpoint;
			fileList_pptFormat.Sort( delegate(string x, string y){
				string result_1 = x.Replace( path + "\\", "");
				string result_2 = result_1.Replace("Slide", "");
				string[] result_3 = result_2.Split(new char[] {'.'});
				int x_index = System.Convert.ToInt32(result_3[0]);

				result_1 = y.Replace( path+ "\\", "");
				result_2 = result_1.Replace("Slide", "");
				result_3 = result_2.Split(new char[] {'.'});
				int y_index = System.Convert.ToInt32(result_3[0]);

				return x_index.CompareTo(y_index);
			});

			//Merging pptFormat slides and unknown Format
			fileList_pptFormat.AddRange (fileList_unknownFormat);
		
			_slides = new List<Texture2D>();

			foreach (string name in fileList_pptFormat)
            {
                byte[] data = File.ReadAllBytes(name);
                _slides.Add(new Texture2D(1, 1));
                _slides[_slides.Count - 1].LoadImage(data);
                _slides[_slides.Count - 1].anisoLevel = 4;
            }
            return true;
        }
    }

    public bool UnLoadSlides()
    {
        if (_isPlaying || _slides == null)
            return false;

        //foreach (Texture2D page in _slides)
        //    Destroy(page);

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

    public bool NextSlide()
    {
        if (!_isPlaying || _isBlending || _currIdx >= _slides.Count - 1)
            return false;
        _currIdx++;
        mat.SetTexture("_NextTex", _slides[_currIdx]);
        StartCoroutine(Blend_CR());
        return true;
    }

    public bool PrevSlide()
    {
        if (!_isPlaying || _isBlending || _currIdx <= 0)
            return false;
        _currIdx--;
        mat.SetTexture("_NextTex", _slides[_currIdx]);
        StartCoroutine(Blend_CR());
        return true;
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
