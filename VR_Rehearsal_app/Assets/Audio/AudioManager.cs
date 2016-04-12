using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _currAudioManager = null;
    public static AudioManager currAudioManager
    {
        get
        {
            if (_currAudioManager == null)
                _currAudioManager = FindObjectOfType<AudioManager>();
            return _currAudioManager;
        }
    }

    public enum SoundType
    {
        Ambient,
        Chat,
    }


    [Serializable]
    public class SoundDict : SerializableDictionary<SoundType, AudioClip>
    {
        public SoundDict() : base() { }
        public SoundDict(IDictionary<SoundType, AudioClip> dict) : base(dict) { }
    }

    [HideInInspector, SerializeField]
    public SoundDict clipDict = new SoundDict();

    public CardboardAudioListener listener;
    public CardboardAudioRoom room;

    //object pools
    public int maxUnitNumber;
    public const int UNIT_NUM_LIMIT = 64;

    private LinkedList<AudioUnit> _unitPool;
    private LinkedListNode<AudioUnit> _nextUnit;

    public void Init()
    {
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
            print("no more free unit");
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
#if UNITY_EDITOR
            print("allocate new unit");
#endif
            return true;
        }
    }

    private void RecycleUnit(AudioUnit unit)
    {
        if (unit == null || !unit.isAllocated)
        {
#if UNITY_EDITOR
            print("try to recycle invalid unit");
#endif
            return;
        }

#if UNITY_EDITOR
        print("recycle unit");
#endif
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

    public bool Allocate3dSound(SoundType type, Transform parent, Vector3 localPos, out AudioUnit unit)
    {
        if (!AllocateUnit(out unit))
            return false;

        unit.source.clip = clipDict[type];
        unit.gameObject.transform.parent = parent != null ? parent : transform;
        unit.gameObject.transform.localPosition = localPos;

        return true;
    }
}

[Serializable]
public class ClipNamePair
{
    public string name;
    public AudioClip clip;
}