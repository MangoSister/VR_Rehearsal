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
    public micManager mic;
    public Stage stage;

	void Start ()
    {
        if (SceneManager.screenTransition != null)
            SceneManager.screenTransition.Fade(true, 1.0f);
    }

}
