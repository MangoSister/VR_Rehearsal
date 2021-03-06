﻿using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using States = Audience.States;

public class AudienceAnimHandlerFull : AudienceAnimHandlerFollow
{
    public static readonly Dictionary<States, int> subStateNum = new Dictionary<States, int>()
    {
        {States.Focused, 3 },
        {States.Bored, 5 },
        {States.Chatting, 1 }
    };

    private static readonly Dictionary<States, float[]> subStateCmf = new Dictionary<States, float[]>()
    {
        {States.Focused, new float[3]{ 0.3f, 0.9f, 1.0f} },
        {States.Bored, new float[5]{0.2f, 0.25f, 0.55f, 0.9f, 1.0f } },
        {States.Chatting, new float[1] { 1.0f } }
    };

    private static readonly Color[] propPalette = new Color[]
    {
        new Color(0.2f, 0.2f, 0.2f),
        new Color(0.8f, 0.8f, 0.8f),
    };

    public static readonly float laptopAnimLength = 0.75f;

    public Vector2 repeatPeriodBound = new Vector2(3f, 8f);


    public GameObject laptop;
    public GameObject smartPhone;


    private Coroutine _currLaptopAnimCR = null;
    private bool _laptopEnlarge = false;

    public override void UpdateStateAnim()
    {
        base.UpdateStateAnim();
        float sample = Random.value;
        int nextSubState = 0;
        for (nextSubState = 0; nextSubState < subStateNum[_audience.currState]; ++nextSubState)
        {
            if (sample < subStateCmf[_audience.currState][nextSubState])
                break;
        }
        controller.SetInteger(_paramIdSubState, nextSubState);
        float nextBlendFactor0 = Random.value;
        controller.SetFloat(_paramIdBlendFactor0, nextBlendFactor0);
        bool nextMirror = Random.value > 0.5f;
        controller.SetBool(_paramIdMirror, nextMirror);
    }

    public override void UpdateChatDirection(float dir)
    {
        base.UpdateChatDirection(dir);
        if (Mathf.Abs((dir - 0.5f) * Mathf.PI) < Mathf.PI * 0.25)
            controller.SetFloat(_paramIdBlendFactor0, 0f);
    }

    private IEnumerator SwitchSubState_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Lerp(repeatPeriodBound.x, repeatPeriodBound.y, Random.value));
            int nextSubState = Random.Range(0, subStateNum[_audience.currState]);
            controller.SetInteger(_paramIdSubState, nextSubState);
        }
    }

    protected override void Awake()
    {
        _audience = GetComponent<Audience>();
        controller = GetComponentInChildren<Animator>();
        controller.runtimeAnimatorController = AudienceAnimWarehouse.curr.fullController;
        controller.SetLayerWeight(defaultLayerIdx, 1f);

        var laptopTransform = (from x in GetComponentsInChildren<Transform>()
                               where x.gameObject.name == "laptop_transform"
                               select x).FirstOrDefault();

        laptop = Instantiate(AudienceAnimWarehouse.curr.laptopPrefab);
        laptop.GetComponentInChildren<MeshRenderer>().material.color = propPalette[Random.Range(0, propPalette.Length)];
        laptop.transform.parent = laptopTransform;
        laptop.transform.localPosition = Vector3.zero;
        laptop.transform.localRotation = Quaternion.identity;
        laptop.SetActive(false);
        var smartphoneTransform = (from x in GetComponentsInChildren<Transform>()
                                   where x.gameObject.name == "smartphone_transform"
                                   select x).FirstOrDefault();

        smartPhone = Instantiate(AudienceAnimWarehouse.curr.smartphonePrefab);
        smartPhone.GetComponent<MeshRenderer>().material.color = propPalette[Random.Range(0, propPalette.Length)];
        smartPhone.transform.parent = smartphoneTransform;
        smartPhone.transform.localPosition = Vector3.zero;
        smartPhone.transform.localRotation = Quaternion.identity;
        smartPhone.transform.localScale = Vector3.one;
        smartPhone.SetActive(false);
    }

    public void AnimateLaptop(bool enlarge)
    {
        _laptopEnlarge = enlarge;
        if (_currLaptopAnimCR == null)
        {
            _currLaptopAnimCR = StartCoroutine(AnimateLaptop_CR());
        }
    }

    private IEnumerator AnimateLaptop_CR()
    {
        laptop.SetActive(true);
        float progress = _laptopEnlarge ? 0f : 1f;
        Vector3 fullScale = laptop.transform.localScale;
        while (progress >= 0f && progress <= 1f)
        {
            progress += (_laptopEnlarge ? 1.0f : -1.0f) * Time.deltaTime;
            float t = MultiSmoothStep( Mathf.Clamp01(progress / laptopAnimLength), 3);
            laptop.transform.localScale = Vector3.Lerp(Vector3.zero, fullScale, t);
            yield return null;
        }

        if (progress < 0f)
            laptop.SetActive(false);

        laptop.transform.localScale = fullScale;

        _currLaptopAnimCR = null;
    }

    private float MultiSmoothStep(float t, int level = 1)
    {
        float ret = t;
        for (int i = 0; i < level; ++i)
            ret = Mathf.Pow(ret, 2) * (3 - 2 * ret);
        return ret;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.J))
    //        AnimateLaptop(true);
    //    if (Input.GetKeyDown(KeyCode.K))
    //        AnimateLaptop(false);
    //}
}
