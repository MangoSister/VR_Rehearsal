using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{

	public  Transform loadingBar;
	public  Transform textIndicator;
	public  Transform textLoading;
    public GameObject doneImg;

	int currentAmount;
	int totalAmount;
	float totalImage;
	float p;
	float t;
	int now;
	[SerializeField]
	float speed;
	public static bool isStart = false;
	// Use this for initialization
	void Start ()
	{
        doneImg.SetActive(false);
		isStart = false;
    }

	void Update ()
	{
		if (isStart) {

			if (currentAmount < totalAmount) {
				doneImg.SetActive (false);
				currentAmount += (int)(speed * Time.deltaTime);
				textIndicator.GetComponent<Text> ().text = ((int)currentAmount * (100 / totalAmount)).ToString () + "%";
				textLoading.gameObject.SetActive (true);
			} else {
				textLoading.gameObject.SetActive (false);
				doneImg.SetActive (true);
				textIndicator.GetComponent<Text> ().text = "Done!";
				isStart = false;
			}

		} 

    }
	public void ResetProgress(){
		doneImg.SetActive(false);
		textIndicator.GetComponent<Text> ().text=  "0 %"; 
	}

	public void StartProgress (int part, int max)
	{

		isStart = true;
		currentAmount = part;
		totalAmount = max;
		now = part;
		p = (float)part;
		t = (float)totalAmount;
		textIndicator.GetComponent<Text> ().text=  "0 %"; 
		loadingBar.GetComponent<Image> ().fillAmount = p / t;

	}


 
}

