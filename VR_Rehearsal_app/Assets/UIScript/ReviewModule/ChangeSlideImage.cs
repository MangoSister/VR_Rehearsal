using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeSlideImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
    public void UpdateImage(Texture2D slideTexture)
    {
        gameObject.GetComponent<Image>().sprite = Sprite.Create(slideTexture, new Rect(0, 0, slideTexture.width, slideTexture.height), new Vector2(0.5f, 0.5f));
    }



	// Update is called once per frame
	void Update () {
	
	}
}
