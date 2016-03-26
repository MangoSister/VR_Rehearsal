using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LogoView : MonoBehaviour {

   public static bool isLogoSceneDone;
    void Start()
    {
        isLogoSceneDone = false;
        GetComponent<RectTransform>().SetAsLastSibling();
        StartCoroutine("ChangePanel");
    }
    public IEnumerator ChangePanel()
    {
        yield return new WaitForSeconds(2.0f);
        isLogoSceneDone = true;
        this.gameObject.SetActive(false);
    }
}
	

