using UnityEngine;
using System.Collections;

public delegate void StageDoorOperated_Handler(bool open);

[RequireComponent(typeof(StageAnimHandler))]
public class Stage : MonoBehaviour
{
    public StageDoorOperated_Handler OnStageDoorOperated;

    private AudioUnit _ambientUnit = null;

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

    private void OperateAmbient(DestType dest)
    {
        StopAllCoroutines();
        if (dest == DestType.PODIUM)
            StartCoroutine(SilenceAfterOpenning_CR());
        else if (dest == DestType.DOOR_OUT)
            OperateAmbient(true);
    }

    private IEnumerator SilenceAfterOpenning_CR()
    {
        while (!RoomCenter.currRoom.mic.isSpeaking)
            yield return null;
        OperateAmbient(false);
    }

    private void Start()
    {
        OperateAmbient(true);
        RoomCenter.currRoom.presenter.OnPostMove += OperateAmbient;
    }
}
