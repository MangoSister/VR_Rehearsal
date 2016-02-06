using UnityEngine;
using System.Collections;

public class AudienceAnimHandler : MonoBehaviour
{
    private Audience _audience { get { return GetComponent<Audience>(); } }
    public Animator anim;

    public Vector2 repeatPeriodBound = new Vector2(3f, 8f);

    private void Start()
    {
        StartCoroutine(Attention_CR());
        StartCoroutine(Repeat_CR());
    }

    private IEnumerator Attention_CR()
    {
        while (true)
        {
            anim.SetInteger("state", (int)_audience.currState);
            yield return null;
        }
    }

    private IEnumerator Repeat_CR()
    {
        while (true)
        {
            float repeatWaitTime = Mathf.Lerp(repeatPeriodBound.x, repeatPeriodBound.y, Random.value);
            yield return new WaitForSeconds(repeatWaitTime);
            if(Random.value > 0.5f)
                anim.SetTrigger("switchPose");
        }
    }
}
