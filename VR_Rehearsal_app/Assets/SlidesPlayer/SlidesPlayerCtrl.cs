/* SlidesPlayerCtrl.cs
 * Yang Zhou, last modified on March 25, 2016
 * The controller of SlidesPlayer, there are two types of control: auto advance & manual control by trigger
 * Dependencies: SlidesPlayer.cs
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlidesPlayerCtrl : MonoBehaviour
{
    public enum SlidesCtrlType
    {
        AutoAdvance, Trigger
    };

    public SlidesCtrlType ctrlType = SlidesCtrlType.Trigger;

    public float autoAdvanceInterval = 60f;

    public float doubleClickInterval = 0.5f;
    private int _clickCounter = 0;

    private List<KeyValuePair<float, int>> _transitionRecord;
    public List<KeyValuePair<float, int>> outputTransitionRecord
    { get { return new List<KeyValuePair<float, int>>(_transitionRecord); } }

    private SlidesPlayer _player { get { return GetComponent<SlidesPlayer>(); } }
    private void Start()
    {
        if (ctrlType == SlidesCtrlType.Trigger)
            Cardboard.SDK.OnTrigger += OnTriggerPulled;
        else if (ctrlType == SlidesCtrlType.AutoAdvance)
            StartCoroutine(AutoAdvance_CR());

        _transitionRecord = new List<KeyValuePair<float, int>>();
        _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, 0));
        _player.Play();
    }

    private IEnumerator AutoAdvance_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoAdvanceInterval);
            _player.NextSlide();
            _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, _player.CurrIdx));
        }
    }

    private void OnTriggerPulled()
    {
        _clickCounter++;
        if (_clickCounter == 1)
            StartCoroutine(DoubleClick_CR());

    }

    private IEnumerator DoubleClick_CR()
    {
        yield return new WaitForSeconds(doubleClickInterval);

        if (_clickCounter == 1)
            _player.NextSlide();
        else if (_clickCounter > 1)
            _player.PrevSlide();

        _transitionRecord.Add(new KeyValuePair<float, int>(Time.time - PresentationData.in_EnterTime, _player.CurrIdx));

        _clickCounter = 0;
    }
}
