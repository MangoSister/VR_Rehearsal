using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using URandom = UnityEngine.Random;

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
        Ambient1,
        Ambient2,
        Whispering1,
        Whispering2,
        CaughingMan,
        CaughingWoman,
        Chair,
        PhoneSMS,
        PhoneVibration1,
        PhoneVibration2,
    }

    public enum SoundCollection
    {
        Ambient,
        Whispering,
        Miscs,
    }

    private static readonly Dictionary<SoundCollection, List<SoundType>> soundRandomCollect
         = new Dictionary<SoundCollection, List<SoundType>>()
         {
             { SoundCollection.Ambient,  new List<SoundType>() { SoundType.Ambient1, SoundType.Ambient2 }  },
             { SoundCollection.Whispering,  new List<SoundType>() { SoundType.Whispering1, SoundType.Whispering2 }  },
             { SoundCollection.Miscs, new List<SoundType>() { SoundType.CaughingMan, SoundType.CaughingWoman, SoundType.Chair, SoundType.PhoneSMS, SoundType.PhoneVibration1, SoundType.PhoneVibration2 } },
         };

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
    public AudioBound miscBound;

    [Range(0f, 1f)]
    public float ambientVolume;
    [Range(0f, 1f)]
    public float miscVolume;
    [Range(0f, 1f)]
    public float chatVolume;

    public Vector2 miscSoundInterval;

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

    public bool AllocateRand3dSound(SoundCollection collection, Transform parent, Vector3 localPos, out AudioUnit unit)
    {
        if (!AllocateUnit(out unit))
            return false;

        SoundType type = soundRandomCollect[collection][URandom.Range(0, soundRandomCollect[collection].Count)];
        unit.source.clip = clipDict[type];
        unit.gameObject.transform.parent = parent != null ? parent : transform;
        unit.gameObject.transform.localPosition = localPos;

        return true;
    }

    public void StartMiscSound()
    {
        StartCoroutine(MiscSound_CR());
    }

    private IEnumerator MiscSound_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(URandom.Range(miscSoundInterval.x, miscSoundInterval.y));
            AudioUnit miscUnit;
            Vector3 pos = new Vector3(URandom.Range(-1f, 1f), URandom.Range(-1f, 1f), URandom.Range(-1f, 1f));
            pos = Vector3.Scale(pos, miscBound.bound.extents);
            pos += miscBound.bound.center;
            if (AllocateRand3dSound(SoundCollection.Miscs, miscBound.transform, pos, out miscUnit))
            {
                miscUnit.source.volume = miscVolume;
                miscUnit.PlayUnloopFadeInout(0f);
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioUnit miscUnit;
            Vector3 pos = new Vector3(URandom.Range(-1f, 1f), URandom.Range(-1f, 1f), URandom.Range(-1f, 1f));
            pos = Vector3.Scale(pos, miscBound.bound.extents);
            pos += miscBound.bound.center;
            if (AllocateRand3dSound(SoundCollection.Miscs, miscBound.transform, pos, out miscUnit))
            {
                miscUnit.source.volume = miscVolume;
                miscUnit.PlayUnloopFadeInout(0f);
            }
        }
    }
#endif
}