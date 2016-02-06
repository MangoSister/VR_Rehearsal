using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager currAudioManager { get { return FindObjectOfType<AudioManager>(); } }

    [SerializeField]
    public List<ClipNamePair> clips;
    private Dictionary<string, AudioClip> _clipDict;

    //object pools
    public int maxUnitNumber;
    public const int UNIT_NUM_LIMIT = 64;

    private LinkedList<AudioUnit> _unitPool;
    private LinkedListNode<AudioUnit> _nextUnit;

    private void Start()
    {
        _clipDict = new Dictionary<string, AudioClip>();
        foreach (var pair in clips)
            _clipDict.Add(pair.name, pair.clip);

        maxUnitNumber = Mathf.Clamp(maxUnitNumber, 0, UNIT_NUM_LIMIT);
        _unitPool = new LinkedList<AudioUnit>();
        for (int i = 0; i < maxUnitNumber; i++)
        {
            GameObject unitObj = new GameObject(string.Format("Audio Unit {0}", i), typeof(AudioUnit));
            unitObj.transform.parent = transform;
            unitObj.SetActive(false);
            AudioUnit unit = unitObj.GetComponent<AudioUnit>();
            _unitPool.AddLast(unit);
        }
        _nextUnit = _unitPool.First;
    }

    private bool AllocateUnit(out AudioUnit unit)
    {
        if (_nextUnit == null)
        {
#if UNITY_EDITOR
            Debug.Log("no more free unit");
#endif
            unit = null;
            return false;
        }
        else
        {
            unit = _nextUnit.Value;
            unit.gameObject.SetActive(true);
            unit.selfNode = _nextUnit;
            unit.OnRecycle += RecycleUnit;
            _nextUnit = _nextUnit.Next;
            return true;
        }
    }

    private void RecycleUnit(AudioUnit unit)
    {
        if (unit == null || !unit.isAllocated)
        {
#if UNITY_EDITOR
            Debug.Log("invalid unit");
#endif
            return;
        }

        LinkedListNode<AudioUnit> node = unit.selfNode;
        unit.selfNode = null;
        unit.OnRecycle -= RecycleUnit;
        unit.gameObject.transform.parent = transform;
        unit.gameObject.transform.localPosition = Vector3.zero;

        if (node.Value.source.isPlaying)
            node.Value.source.Stop();
        node.Value.StopAllCoroutines();
        node.Value.gameObject.SetActive(false);
        _unitPool.Remove(node);
        _unitPool.AddLast(node);
        if (_nextUnit == null)
            _nextUnit = node;
    }

    private void RecycleAll()
    {
        List<AudioUnit> allocated = new List<AudioUnit>();
        foreach (AudioUnit unit in _unitPool)
            if (unit.isAllocated)
                allocated.Add(unit);

        foreach (AudioUnit unit in allocated)
            RecycleUnit(unit);
    }

    private void OnDestroy()
    {
        RecycleAll();
    }

    public void Play3dSound(string clipName, float volume, Transform parent, Vector3 pos, float fade)
    {
        AudioClip clip;
        if (_clipDict.TryGetValue(clipName, out clip))
            Play3dSound(clip, volume, parent, pos, fade);
    }

    public void Play3dSound(string clipName, float volume, Transform parent, Vector3 pos, float fade, ref AudioUnit unit)
    {
        AudioClip clip;
        if (_clipDict.TryGetValue(clipName, out clip))
            Play3dSound(clip, volume, parent, pos, fade, ref unit);
    }

    public void Play3dSound(AudioClip clip, float volume, Transform parent, Vector3 pos, float fade)
    {
        AudioUnit unit;
        if (!AllocateUnit(out unit))
            return;

        unit.source.spatialBlend = 1.0f;
        unit.source.clip = clip;
        unit.source.volume = Mathf.Clamp01(volume);
        unit.gameObject.transform.parent = parent != null ? parent : transform;
        unit.gameObject.transform.localPosition = pos;

        unit.Play(fade);
    }

    public void Play3dSound(AudioClip clip, float volume, Transform parent, Vector3 pos, float fade, ref AudioUnit unit)
    {
        if (!AllocateUnit(out unit))
            return;

        unit.source.spatialBlend = 1.0f;
        unit.source.clip = clip;
        unit.source.volume = Mathf.Clamp01(volume);
        unit.gameObject.transform.parent = parent != null ? parent : transform;
        unit.gameObject.transform.localPosition = pos;

        unit.Play(fade);
    }

    public void Play2dSound(string clipName, float volume, float fade)
    {
        AudioClip clip;
        if (_clipDict.TryGetValue(clipName, out clip))
            Play2dSound(clip, volume, fade);
    }

    public void Play2dSound(AudioClip clip, float volume, float fade)
    {
        AudioUnit unit;
        if (!AllocateUnit(out unit))
            return;

        unit.source.spatialBlend = 0.0f;
        unit.source.clip = clip;
        unit.source.volume = Mathf.Clamp01(volume);

        unit.Play(fade);
    }
}

[Serializable]
public class ClipNamePair
{
    public string name;
    public AudioClip clip;
}