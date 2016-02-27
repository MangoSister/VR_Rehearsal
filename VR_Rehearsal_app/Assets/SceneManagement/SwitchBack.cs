using UnityEngine;
using System.Collections;

public class SwitchBack : MonoBehaviour
{
	IEnumerator Start ()
    {
        yield return new WaitForSeconds(2.0f);
        GlobalObjManager.LaunchPreparationScene();
	}	
}
