using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CalibrationView : MonoBehaviour
{

	public static bool isCalibrationDone;

	public GameObject popUpWindow;
	public GameObject timerBar;
	public GameObject button;
	public GameObject contentText;
	public GameObject descriptionPanel;
	public GameObject circularProgress;

	float rate = 20f;
	float max_time = 100f;
	float curr_time = 0f;
	float calc_timer;

	bool silentFlag = false;
	bool isSilentCalibrationDone = false;
	bool isMicroCalibrationDone = false;
	bool isBetwwen = false;

	// Use this for initialization
	void Start ()
	{
		isCalibrationDone = false;
		descriptionPanel.SetActive(false);
		circularProgress.SetActive(false);
		curr_time = 0;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (silentFlag == true) {
			IncreaseTimer ();
		}
	}

	void IncreaseTimer ()
	{
		if (curr_time < 100) {
			curr_time += (Time.deltaTime * rate);
			circularProgress.GetComponent<RectTransform>().FindChild("loading").GetComponent<Image>().fillAmount = curr_time/max_time;
			button.GetComponent<Button>().interactable = false;

		} else {
			circularProgress.GetComponent<RectTransform>().FindChild("loading").GetComponent<Image>().fillAmount =  0;
			silentFlag = false;
			button.GetComponent<Button>().interactable = true;
			ChangeTheText ();
		}
		isSilentCalibrationDone = true;
	}


	public void PopUpOKButtonClick ()
	{
		popUpWindow.SetActive (false);
		descriptionPanel.SetActive(true);
		circularProgress.SetActive(true);
	}

	public void PopUpSkipButtonClick ()
	{

		popUpWindow.SetActive (false);
		gameObject.SetActive (false);
		isCalibrationDone = true;
	}

	public void CalirbartionStartButtonClick ()
	{
		//First Silence button
		if (isSilentCalibrationDone == false && isMicroCalibrationDone == false && button.GetComponentInChildren<Text> ().text == "Calibration Start") {
			if (silentFlag == false) {
				silentFlag = true;
			}
		} 
		//First Done Button
		else if (isSilentCalibrationDone == true && isMicroCalibrationDone == false && button.GetComponentInChildren<Text> ().text == "Done !" ){
			Debug.Log("2");
			button.GetComponentInChildren<Text> ().text = "Calibration Start";
			contentText.GetComponent<Text>().text = "please test your microphone. Say something shit";

		}
		//Microbutton
		else if(isSilentCalibrationDone == true && isMicroCalibrationDone == false && button.GetComponentInChildren<Text> ().text == "Calibration Start"){
			curr_time = 0;
			silentFlag = true;
			isMicroCalibrationDone = true;
		}
		//2nd Done Button
		else if(isSilentCalibrationDone == true && isMicroCalibrationDone == true){
			Debug.Log("Rotation!!!!!");
			popUpWindow.SetActive (false);
			gameObject.SetActive (false);
			isCalibrationDone = true;
		}

	}

	void ChangeTheText ()
	{
		button.GetComponentInChildren<Text> ().text = "Done !";

	}

	
}
