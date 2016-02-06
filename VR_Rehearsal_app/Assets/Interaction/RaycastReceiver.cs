using UnityEngine;
using System;
using System.Collections;

public delegate void EyeStare_Handler(RaycastReceiver receiver, float progress);
public delegate void EyeEnter_Handler(RaycastReceiver receiver);
public delegate void EyeExit_Handler(RaycastReceiver receiver);

public enum RaycastReceiverType { ONE_TIME, SINGLE_SHOT, CONTINUOUS }

public class RaycastReceiver : MonoBehaviour
{
    public event EyeStare_Handler eyeStared;
    public event EyeEnter_Handler eyeEntered;
    public event EyeExit_Handler eyeExited;
    public RaycastReceiverType reactionType;
    public float stareTime;

    private bool _switch;
    private bool _stare;
    private float _stareTimer;

    protected void Start()
    {
        _switch = true;
        _stare = false;
        _stareTimer = 0f;
    }

    protected void Update()
    {
        if (_stare && _switch)
        {
            _stareTimer += Time.deltaTime;
            if (eyeStared != null)
                eyeStared(this, Mathf.Clamp01(_stareTimer / stareTime));
            if (_stareTimer > stareTime && reactionType != RaycastReceiverType.CONTINUOUS)
                _switch = false;
        }
        else _stareTimer = 0f;
    }

    public void OnEyeEnter()
    {
        _stare = true;
        _stareTimer = 0f;
        if (eyeEntered != null)
            eyeEntered(this);

    }

    public void OnEyeExit()
    {
        _stare = false;
        if (reactionType != RaycastReceiverType.ONE_TIME)
            _switch = true;
        if (eyeExited != null)
            eyeExited(this);
    }
}
