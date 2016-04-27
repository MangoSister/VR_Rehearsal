using UnityEngine;
using System.Collections;

public class ClockTimer : MonoBehaviour
{
    public TextMesh textMesh;
    public float _maxSecond = 0;
    public float extraSecond = 600;
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
        enabled = false;
    }

    public void StartCounting()
    {
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
            if (_currSecond <= 0)
                break;
            yield return new WaitForSeconds(1f);
            _currSecond--;
            int minutes = (int)_currSecond / 60;
            int seconds = (int)_currSecond % 60;
            textMesh.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            if (_currSecond < 0.25f * _maxSecond)
                textMesh.color = Color.red;
        }

        _currSecond = extraSecond;
        while (true)
        {
            if (_currSecond < 0)
                break;
            yield return new WaitForSeconds(1f);
            _currSecond--;
            int minutes = (int)_currSecond / 60;
            int seconds = (int)_currSecond % 60;
            textMesh.text = string.Format("Exit Scene in: \n {0:00}:{1:00}", minutes, seconds);
        }

        SceneController.currRoom.inputManager.ForceEndPractice();
    }
}
