using UnityEngine;
using System.Collections;

public class ClockTimer : MonoBehaviour
{
    public TextMesh textMesh;
    public Transform infoTransform;
    public Transform timerTransform;
    public string startInfo;
    public string endInfo;
    public float _maxSecond = 0;
    private float _currSecond = 0f;

    public void SetMaxTime(int time)
    {
        _maxSecond = time;
        _currSecond = _maxSecond;
    }

    public void ResetTime()
    {
        _currSecond = _maxSecond;
    }

    private void Awake()
    {
        textMesh.text = startInfo;
        enabled = false;
        textMesh.transform.parent = infoTransform;
        textMesh.transform.localPosition = Vector3.zero;
        textMesh.transform.localRotation = Quaternion.identity;
    }

    public void StartCounting()
    {
        textMesh.transform.parent = timerTransform;
        textMesh.transform.localPosition = Vector3.zero;
        textMesh.transform.localRotation = Quaternion.identity;
        StartCoroutine(Timing_CR());
    }

    public void StopCounting()
    {
        StopAllCoroutines();
    }

    private IEnumerator Timing_CR()
    {
        while (true)
        {
            if (_currSecond < 0)
                break;
            yield return new WaitForSeconds(1f);
            _currSecond--;
            int minutes = (int)_currSecond / 60;
            int seconds = (int)_currSecond % 60;
            textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            if (_currSecond < 0.25f * _maxSecond)
                textMesh.color = Color.red;
        }

        textMesh.text = endInfo;
    }
}
