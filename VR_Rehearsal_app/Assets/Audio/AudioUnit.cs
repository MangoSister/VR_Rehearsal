using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void AudioUnitRecycle_Handler(AudioUnit unit);

[RequireComponent(typeof(CardboardAudioSource))]
public class AudioUnit : MonoBehaviour
{
    [HideInInspector]
    public LinkedListNode<AudioUnit> selfNode;
    public bool isAllocated { get { return selfNode != null; } }

    public event AudioUnitRecycle_Handler OnRecycle;

    public CardboardAudioSource source
    {
        get
        {
            var src = GetComponent<CardboardAudioSource>();
            if (src == null)
                return gameObject.AddComponent<CardboardAudioSource>();
            else return src;
        }
    }

    public bool Play()
    {
        if (!isAllocated)
            return false;

        source.Play();
        return true;
    }

    public bool PlayUnloopFadeInout(float fadeTime)
    {
        if (!isAllocated)
            return false;

        source.Play();
        if (!source.loop)
            StartCoroutine(MaintainAndRecycle(source.clip.length, fadeTime));
        return true;
    }

    public bool PlayUnloopFadeInout(float lifeSpan, float fadeTime)
    {
        if (!isAllocated)
            return false;

        source.Play();
        StartCoroutine(MaintainAndRecycle(lifeSpan, fadeTime));
        return true;
    }

    public bool StopAndRecycle()
    {
        if (!isAllocated)
            return false;

        if (OnRecycle != null)
            OnRecycle(this);
        return true;
    }


    public bool StopFadeAndRecycle(float fadeTime)
    {
        if (!isAllocated)
            return false;

        StartCoroutine(FadeAndRecycle(fadeTime));
        return true;
    }


    private IEnumerator FadeAndRecycle(float fadeTime)
    {
        float time = 0f;
        float maxVol = source.volume;
        while (time < fadeTime)
        {
            source.volume = Mathf.Lerp(maxVol, 0.0f, Mathf.Clamp01(time / fadeTime));
            time += Time.deltaTime;
            yield return null;
        }

        if (OnRecycle != null)
            OnRecycle(this);
    }

    private IEnumerator MaintainAndRecycle(float lifeSpan, float fadeTime)
    {
        float maxVol = source.volume;
        if (2 * fadeTime < lifeSpan)
        {
            float time = 0f;
            while (time < fadeTime)
            {
                source.volume = Mathf.Lerp(0.0f, maxVol, Mathf.Clamp01(time / fadeTime));
                time += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(lifeSpan - 2 * fadeTime);
            time = 0f;
            while (time < fadeTime)
            {
                source.volume = Mathf.Lerp(maxVol, 0.0f, Mathf.Clamp01(time / fadeTime));
                time += Time.deltaTime;
                yield return null;
            }
        }
        else if (fadeTime < lifeSpan)
        {
            float time = 0f;
            while (time < fadeTime)
            {
                source.volume = Mathf.Lerp(0.0f, maxVol, Mathf.Clamp01(time / fadeTime));
                time += Time.deltaTime;
                yield return null;
            }
            time = 0f;
            while (time < lifeSpan - fadeTime)
            {
                source.volume = Mathf.Lerp(maxVol, 0.0f, Mathf.Clamp01(time / fadeTime));
                time += Time.deltaTime;
                yield return null;
            }
            //0 - fade time fade in
            //fade time - life span fade out
        }
        else
        {
            //fade out Life Span
            float time = 0f;
            while (time < lifeSpan)
            {
                source.volume = Mathf.Lerp(maxVol, 0.0f, Mathf.Clamp01(time / fadeTime));
                time += Time.deltaTime;
                yield return null;
            }
        }

        if (OnRecycle != null)
            OnRecycle(this);
    }
}
