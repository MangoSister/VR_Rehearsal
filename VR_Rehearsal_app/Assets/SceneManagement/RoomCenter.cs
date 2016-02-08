using UnityEngine;
using System.Collections;

public class RoomCenter : MonoBehaviour
{
    public static RoomCenter currRoom
    { get { return FindObjectOfType<RoomCenter>(); } }

    public Transform presentDest;
    public Transform roomDoorIn;
    public Transform roomDoorOut;

    public PresenterMovement presenter;
    public Transform presenterHead;
    public micManager mic;
    public Stage stage;

    private AudioUnit _ambientUnit = null;

    void Start ()
    {
#if UNITY_ANDROID
     Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif 

        if (SceneManager.screenTransition != null)
            SceneManager.screenTransition.Fade(true, 1.0f);

        OperateAmbient(true);
        StartCoroutine(SilenceAfterOpenning_CR());
        //currRoom.presenter.OnPostMove += OperateAmbient;
    }


    private void OperateAmbient(bool enable)
    {
        if (enable)
            AudioManager.currAudioManager.Play3dSound("Unrest", 1.0f, transform, Vector3.zero, 2.0f, ref _ambientUnit);
        else
        {
            _ambientUnit.Stop(2.0f);
            _ambientUnit = null;
        }
    }

    //private void OperateAmbient(DestType dest)
    //{
    //    StopAllCoroutines();
    //    if (dest == DestType.PODIUM)
    //        StartCoroutine(SilenceAfterOpenning_CR());
    //    else if (dest == DestType.DOOR_OUT)
    //        OperateAmbient(true);
    //}



    private IEnumerator SilenceAfterOpenning_CR()
    {
        while (!currRoom.mic.isSpeaking)
            yield return null;
        OperateAmbient(false);
    }

}
