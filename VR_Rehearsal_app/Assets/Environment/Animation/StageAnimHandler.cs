using UnityEngine;
using System.Collections;

public class StageAnimHandler : MonoBehaviour
{
    private Stage _stage { get { return GetComponent<Stage>(); } }
    public Animator anim;

    private void Start()
    {
        _stage.OnStageDoorOperated += TriggerDoor;
    }

    private void TriggerDoor(bool open)
    {
        if (open)
            anim.SetTrigger("openDoor");
        else anim.SetTrigger("closeDoor");
    }
}
