using UnityEngine;
using System.Collections;

public delegate void StageDoorOperated_Handler(bool open);

[RequireComponent(typeof(StageAnimHandler))]
public class Stage : MonoBehaviour
{
    public StageDoorOperated_Handler OnStageDoorOperated;

    public void OperateDoor(bool open)
    {
        if (OnStageDoorOperated != null)
            OnStageDoorOperated(open);
    }

    public void WalkThroughDoorAction(bool dir, bool open)
    {
        if (dir && open)
            OperateDoor(true);
        else if (!dir && !open)
            OperateDoor(false);
    }
}
