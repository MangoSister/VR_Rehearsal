using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RaycastReceiver))]
public class EyegazeCube : MonoBehaviour
{
    public DestType dest;

    private RaycastReceiver _receiver
    { get { return GetComponent<RaycastReceiver>(); } }

    private Material _mat
    { get { return GetComponent<MeshRenderer>().material; } }

    private Color _initColor;

	// Use this for initialization
	private void Start ()
    {
        _receiver.eyeStared += ReceiverEyeStared;
        _initColor = _mat.color;
	}

    private void ReceiverEyeStared(RaycastReceiver receiver, float progress)
    {
        _mat.color = Color.Lerp(_initColor, Color.white, progress);
        if (progress >= 1f)
            RoomCenter.currRoom.presenter.MoveTo(dest);
    }

}
