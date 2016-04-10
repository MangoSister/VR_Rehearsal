/* TriggerCtrl.cs
 * Yang Zhou, last modified on April 09, 2016
 * The controller of SlidesPlayer, there are two types of control: auto advance & manual control by trigger
 * Dependencies: SlidesPlayer.cs
 */

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public enum SlidesCtrlType
    {
        AutoAdvance, Trigger
    };

    public SlidesCtrlType ctrlType = SlidesCtrlType.Trigger;

    public float autoAdvanceInterval = 60f;

    private List<KeyValuePair<float, int>> _transitionRecord;
    public List<KeyValuePair<float, int>> outputTransitionRecord
    { get { return new List<KeyValuePair<float, int>>(_transitionRecord); } }

    public SlidesPlayer player;

    public delegate void ExitHandler();
    public ExitHandler OnExit;

    public float exitInterval = 3f;
    public MeshRenderer exitRenderer;
    
    public float doubleClickInterval = 0.5f;
    private bool _startDetect = false;

    private bool _beginPractice = false;
    public delegate void BeginPracticeHandler();
    public BeginPracticeHandler OnPracticeBegin;

    private void Start()
    {
        if (ctrlType == SlidesCtrlType.Trigger)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            Cardboard.SDK.OnTrigger += OnTriggerPulled;
#endif
        }
        else if (ctrlType == SlidesCtrlType.AutoAdvance)
            StartCoroutine(AutoAdvance_CR());

        _transitionRecord = new List<KeyValuePair<float, int>>();
        _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, 0));
        player.Play();

        exitRenderer.enabled = false;
        _beginPractice = false;
    }

    private IEnumerator AutoAdvance_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoAdvanceInterval);
            player.NextSlide();
            _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, player.CurrIdx));
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetMouseButton(0))
            OnTriggerPulled();
    }
#endif

    private void OnTriggerPulled()
    {
        if (!_startDetect)
        {
            _startDetect = true;
            StartCoroutine(DoubleClick_CR());

        }
    }

    private IEnumerator DoubleClick_CR()
    {
        int touchCounter = 0;
        float time = 0f;
        while (time < doubleClickInterval)
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
                touchCounter++;
#else
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended)
                    touchCounter++;
            }
#endif
            time += Time.deltaTime;
            yield return null;
        }

        if(_beginPractice)
        {
            if (touchCounter == 1) //single touches
            {
                player.NextSlide();
                _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, player.CurrIdx));
                _startDetect = false;
            }
            else if (touchCounter > 1) //double touches
            {
                player.PrevSlide();
                _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, player.CurrIdx));
                _startDetect = false;
            }
            else
            {
                //hold
                yield return StartCoroutine(Hold_CR());
            }
        }
        else
        {
            _beginPractice = true;
            if (OnPracticeBegin != null)
                OnPracticeBegin();
            _startDetect = false;
        }
    }

    private IEnumerator Hold_CR()
    {
        exitRenderer.enabled = true;
        bool success = true;
        float time = 0f;
        while (time < exitInterval)
        {
            time += Time.deltaTime;
            exitRenderer.material.SetFloat("_Angle", Mathf.Lerp(-Mathf.PI, Mathf.PI, Mathf.Clamp01(time / exitInterval)));

#if UNITY_EDITOR
            if (Input.GetMouseButtonUp(0))
            {
                success = false;
                break;
            }
#else
            foreach (var touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Ended)
                {
                    success = false;
                    break;
                }

            }

            if (!success)
                break;
#endif


            yield return null;
        }


        if (success && OnExit != null)
        {
            OnExit();
        }

        _startDetect = false;
        exitRenderer.enabled = false;
    }
}
