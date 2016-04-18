using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MangoBehaviorTree;
using CrowdConfigInfo = spaceInfoParser.parsedData_spaceInfo;
using LOD = Audience.DetailLevel;
using URandom = UnityEngine.Random;

public class CrowdSimulator : MonoBehaviour
{
    [Flags]
    public enum SimModule
    {
        Gaze = 1,
        VoiceVolume = 2,
        FillerWords = 4,
        SeatDistribution = 8,
        SocialGroup = 16,
        Global = 32,
    };

    private static CrowdSimulator _currSim = null;
    public static CrowdSimulator currSim
    {
        get
        {
            if (_currSim == null)
                _currSim = FindObjectOfType<CrowdSimulator>();
            return _currSim;
        }
    }

    public const int characterNum = 14;
    public Animator[] fullSizeBody;
    public Animator[] halfSizeBody;
    public Vector3 asseblingPos = new Vector3(0f, -0.31f, 0f);
    public Quaternion asseblingRot = Quaternion.Euler(0f, -180f, 0f);
    public Shader bumpShader;
    public Shader diffuseShader;
    public Texture2D[] audienceClothAlbedos;
    public Texture2D[] audienceSkinAlbedos;
    public Texture2D[] audienceClothNrms;
    public Texture2D[] audienceSkinNrms;
    public Material[] audienceClothMaterials;
    public Material[] audienceSkinMaterials;

    public GameObject prefabEyeIcon;

    public Transform crowdParent;
    private List<int> _characterIdxShuffledList;
    private int _nextCharacterIdx;

    public string crowdConfigFileName;
    public float stepIntervalInt
    {
        get
        { return _stepIntervalInt; }
        set
        {
            _stepIntervalInt = value;
            if (_waitNode == null)
                _waitNode = new WaitNode<Audience>(value);
            else _waitNode.waitTime = value;
        }
    }

    private float _stepIntervalInt;
    private WaitNode<Audience> _waitNode;

    public float stepIntervalExt;
    public float stepIntervalInput;

    public SimModule simModule = 
        SimModule.Gaze | SimModule.VoiceVolume | 
        SimModule.FillerWords | SimModule.SeatDistribution | 
        SimModule.SocialGroup | SimModule.Global;

    public bool deterministic;

    public float globalAttentionMean;
    public float globalAttentionStDev;
    public float globalAttentionAmp;
    public float globalAttentionConstOffset;
    public float[] globalInternalUpdatePeriod = new float[3];

    public AnimationCurve globalTimeCurve;

    public AnimationCurve seatPosAttentionCurve;
    public float seatPosAttentionUpper
    {
        get
        {
            return seatPosAttentionCurve.keys[0].value;
        }
        set
        {
            var keys = seatPosAttentionCurve.keys;
            keys[0].value = value;
            seatPosAttentionCurve = new AnimationCurve(keys);

        }
    }
    public float seatPosAttentionLower
    {
        get
        {
            return seatPosAttentionCurve.keys[1].value;
        }
        set
        {
            var keys = seatPosAttentionCurve.keys;
            keys[1].value = value;
            seatPosAttentionCurve = new AnimationCurve(keys);
        }
    }

    public SimpleGazeCollision gazeCollision;
    public RecordingWrapper recordWrapper;

    [Range(0f,1f)]
    public float gazeCumulativeIntensity;

    public float voiceUpdatePeriod;
    public float fluencySignificantThreshold;
    public AnimationCurve fluencyCurve;

    private List<Audience> audiences;
    public int audienceNum { get { return audiences.Count; } }
    private List<SocialGroup> socialGroups;
    public float noChatThreshold;
    public float avgChatThreshold;
    public float chatLength;
    public float genChatPeriod;

    //private BehaviorTree<Audience> _audienceBt = null;
    private BehaviorTree<Audience> _behaviorTree;

    private void CreateBehaviorTree()
    {
        if (_waitNode == null)
            _waitNode = new WaitNode<Audience>(stepIntervalInt);
        _behaviorTree = new BehaviorTree<Audience>
            (new SequenceNode<Audience>
                (new SelectorNode<Audience>
                    (new SequenceNode<Audience>
                        (new InstantSuccessModifier<Audience>(_waitNode),
                        new AudienceInternalSimNode()),
                    new AudienceBypassInternalNode()),
                new AudienceExternalSimNode(),
                new AudienceStateSelectorNode
                    (1, 
                    new AudienceFocusStateNode(), 
                    new AudienceBoredStateNode(), 
                    new AudienceChatStateNode())));
    }

    private void CreateCrowd()
    {
        //instantiate audience
        CrowdConfigInfo tx = spaceInfoParser.Parse(crowdConfigFileName);
        audiences = new List<Audience>();
        _characterIdxShuffledList = new List<int>(characterNum);
        for (int i = 0; i < characterNum; ++i)
            _characterIdxShuffledList.Add(i);
        Shuffle(_characterIdxShuffledList);
        _nextCharacterIdx = 0;

        for (int i = 0; i < tx.seat_RowNum * tx.seat_ColNum; i++)
        {
            Audience ad;
            int row = i % tx.seat_RowNum;
            if (row < 2)
                ad = CreateRandomMember(LOD.FullSize_Diffuse_FullAnim, Vector3.zero, Quaternion.identity);
            else if (row < 3)
                ad = CreateRandomMember(LOD.FullSize_Diffuse_FollowAnim, Vector3.zero, Quaternion.identity);
            else
                ad = CreateRandomMember(LOD.HalfSize_Diffuse_BasicAnim, Vector3.zero, Quaternion.identity);

            ad.simInternalOffset = URandom.Range(0, stepIntervalInt);
            ad.followingTransform = SceneController.currRoom.presenterHead;

            //to Phan: fix the layout here
            ad.normalizedPos = (float)(i % tx.seat_ColNum) / (float)tx.seat_ColNum;
            ad.transform.parent = crowdParent;
            ad.transform.localPosition = tx.seat_posVecs[i];
            ad.transform.localRotation = tx.seat_rotQuans[i];
            audiences.Add(ad);
        }

        //create social group
        socialGroups = new List<SocialGroup>();
        for (int i = 0; i < audiences.Count; i++)
        {
            if (URandom.value > 0.3f)
                continue;

            int col = i / tx.seat_RowNum;
            int row = i % tx.seat_RowNum;
            int idx = col * tx.seat_RowNum + row;

            if (audiences[idx].socialGroup != null)
                continue;

            List<Audience> neighbors = new List<Audience>();
            //randomly create social groups for now, 8 connectivity neighbors
            if (row < tx.seat_RowNum - 1 && audiences[col * tx.seat_RowNum + row + 1].socialGroup == null && URandom.value < 0.1f)
                neighbors.Add(audiences[col * tx.seat_RowNum + row + 1]);
            if (row > 0 && audiences[col * tx.seat_RowNum + row - 1].socialGroup == null && URandom.value < 0.1f)
                neighbors.Add(audiences[col * tx.seat_RowNum + row - 1]);
            if (col < tx.seat_ColNum - 1 && audiences[(col + 1) * tx.seat_RowNum + row].socialGroup == null && URandom.value < 0.25f)
                neighbors.Add(audiences[(col + 1) * tx.seat_RowNum + row]);
            if (col > 0 && audiences[(col - 1) * tx.seat_RowNum + row].socialGroup == null && URandom.value < 0.25f)
                neighbors.Add(audiences[(col - 1) * tx.seat_RowNum + row]);

            if (row < tx.seat_RowNum - 1 && col < tx.seat_ColNum - 1 && audiences[(col + 1) * tx.seat_RowNum + row + 1].socialGroup == null && URandom.value < 0.05f)
                neighbors.Add(audiences[(col + 1) * tx.seat_RowNum + row + 1]);
            if (row > 0 && col < tx.seat_ColNum - 1 && audiences[(col + 1) * tx.seat_RowNum + row - 1].socialGroup == null && URandom.value < 0.05f)
                neighbors.Add(audiences[(col + 1) * tx.seat_RowNum + row - 1]);
            if (row < tx.seat_RowNum - 1 && col > 0 && audiences[(col - 1) * tx.seat_RowNum + row + 1].socialGroup == null && URandom.value < 0.05f)
                neighbors.Add(audiences[(col - 1) * tx.seat_RowNum + row + 1]);
            if (row > 0 && col > 0 && audiences[(col - 1) * tx.seat_RowNum + row - 1].socialGroup == null && URandom.value < 0.05f)
                neighbors.Add(audiences[(col - 1) * tx.seat_RowNum + row - 1]);

            if (neighbors.Count > 0)
            {
                neighbors.Add(audiences[idx]);
                var groupObj = new GameObject("Social Group", typeof(SocialGroup));
                groupObj.transform.parent = transform;
                groupObj.GetComponent<SocialGroup>().members = neighbors;
                socialGroups.Add(groupObj.GetComponent<SocialGroup>());
                foreach (Audience person in neighbors)
                    person.socialGroup = groupObj.GetComponent<SocialGroup>();
            }

        }
    }

    public void Init()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        stepIntervalInt = globalInternalUpdatePeriod[0];

        if (_behaviorTree == null)
            CreateBehaviorTree();

        CreateCrowd();
    }

    public void StartSimulation()
    {
        if (_behaviorTree != null)
        {
            StartCoroutine(Simulate_CR());
            StartCoroutine(UpdateSocialGroup_CR());
            StartCoroutine(UpdateGazeEffect_CR());
            StartCoroutine(UpdateVoice_CR());
            StartCoroutine(UpdateStepIntervalInt_CR());
        }
    }

    public void StopSimulation()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateStepIntervalInt_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(stepIntervalInt);
            float passedTime = Time.time - PresentationData.in_EnterTime;
            if (passedTime > 0.66f * PresentationData.in_ExpectedTime)
            {
                stepIntervalInt = globalInternalUpdatePeriod[2];
#if UNITY_EDITOR
                print(string.Format("new period: {0}", stepIntervalInt));
#endif
                break;
            }
            else if (passedTime > 0.33f * PresentationData.in_ExpectedTime)
            {
                stepIntervalInt = globalInternalUpdatePeriod[1];
#if UNITY_EDITOR
                print(string.Format("new period: {0}", stepIntervalInt));
#endif
            }
        }
    }

    private IEnumerator UpdateGazeEffect_CR()
    {
        while (true)
        {
            gazeCollision.UpdateGazeContact();
            yield return new WaitForSeconds(stepIntervalInput);
        }
    }

    private IEnumerator UpdateSocialGroup_CR()
    {
        while (true)
        {
            socialGroups[URandom.Range(0, socialGroups.Count)].UpdateChatStatus();
            yield return new WaitForSeconds(genChatPeriod);
        }
    }

    private IEnumerator UpdateVoice_CR()
    {
        while (true)
        {
            yield return new WaitForSeconds(voiceUpdatePeriod);
            recordWrapper.UpdateFluencyScore();
            if (recordWrapper.fluencyDelta > fluencySignificantThreshold)
            {
                foreach (Audience ad in audiences)
                    ad.lazyUpdateLock = true;
            }
        }
    }

    private IEnumerator Simulate_CR()
    {
        //init round
        for (int i = 0; i < audienceNum; ++i)
            _behaviorTree.NextTick(audiences[i]);

        //update rounds
        while (true)
        {
            Shuffle(audiences);
            //(audiences);
            for (int i = 0; i < audienceNum; ++i)
            {
                _behaviorTree.NextTick(audiences[i]);
                yield return new WaitForSeconds(stepIntervalExt);
            }
        }
    }

    private void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = URandom.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public Audience CreateRandomMember(LOD lod, Vector3 pos, Quaternion rot)
    {
        
        switch (lod)
        {
            case LOD.FullSize_Bump_FullAnim:
                {
                    throw new NotImplementedException();
                }
            case LOD.FullSize_Diffuse_FullAnim:
                {
                    int idx = _characterIdxShuffledList[(_nextCharacterIdx++) % characterNum];
                    GameObject adObj = new GameObject("Lv "+ (int)lod + " agent", typeof(Audience));
                    Animator body = Instantiate(fullSizeBody[idx]) as Animator;
                    body.transform.parent = adObj.transform;
                    body.transform.localPosition = asseblingPos;
                    body.transform.localRotation = asseblingRot;

                    Audience ad = adObj.GetComponent<Audience>();
                    adObj.name += " " + ad.agentId;
                    ad.headTransform = (from x in ad.transform.GetComponentsInChildren<Transform>()
                                        where x.gameObject.name == "Head_wrapper" || x.gameObject.name == "HeadWrapper"
                                        select x).FirstOrDefault();
                    ad.detailLevel = lod;

                    adObj.AddComponent(typeof(AudienceAnimHandlerFull)); //template version will crash Unity...
                    var handler = adObj.GetComponent<AudienceAnimHandlerFull>();
                    handler.controller = body;
                    handler.LerpAnimLayerSpeed = 2f;
                    handler.repeatPeriodBound = new Vector2(30f, 60f);
                    handler.SwitchFollowDegSpeed = URandom.Range(80f, 120f);

                    var icon = Instantiate<GameObject>(prefabEyeIcon) as GameObject;
                    icon.SetActive(false);
                    icon.transform.parent = ad.headTransform;
                    icon.transform.localPosition = AudienceAnimHandlerBasic.eyeIconOffset;
                    ad.GetComponent<AudienceAnimHandlerFull>().eyeIcon = icon;

                    Material clothMat, skinMat;
                    GetAudienceMat(false, idx, out clothMat, out skinMat);
                    body.transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material = skinMat;
                    body.transform.Find("Shoes").GetComponent<SkinnedMeshRenderer>().material = clothMat;

                    ad.transform.position = pos;
                    ad.transform.rotation = rot;

                    return ad;
                }
            case LOD.FullSize_Diffuse_FollowAnim:
                {
                    int idx = _characterIdxShuffledList[(_nextCharacterIdx++) % characterNum];
                    GameObject adObj = new GameObject("Lv " + (int)lod + " agent", typeof(Audience));
                    Animator body = Instantiate(fullSizeBody[idx]) as Animator;
                    body.transform.parent = adObj.transform;
                    body.transform.localPosition = asseblingPos;
                    body.transform.localRotation = asseblingRot;

                    Audience ad = adObj.GetComponent<Audience>();
                    adObj.name += " " + ad.agentId;
                    ad.headTransform = (from x in ad.transform.GetComponentsInChildren<Transform>()
                                        where x.gameObject.name == "Head_wrapper" || x.gameObject.name == "HeadWrapper"
                                        select x).FirstOrDefault();
                    ad.detailLevel = lod;

                    adObj.AddComponent(typeof(AudienceAnimHandlerFollow)); //template version will crash Unity...
                    var handler = adObj.GetComponent<AudienceAnimHandlerFollow>();
                    handler.controller = body;
                    handler.LerpAnimLayerSpeed = 2f;
                    handler.SwitchFollowDegSpeed = URandom.Range(80f, 120f);

                    var icon = Instantiate<GameObject>(prefabEyeIcon) as GameObject;
                    icon.transform.parent = ad.headTransform;
                    icon.SetActive(false);
                    icon.transform.localPosition = AudienceAnimHandlerBasic.eyeIconOffset;
                    ad.GetComponent<AudienceAnimHandlerFollow>().eyeIcon = icon;

                    Material clothMat, skinMat;
                    GetAudienceMat(false, idx, out clothMat, out skinMat);
                    body.transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material = skinMat;
                    body.transform.Find("Shoes").GetComponent<SkinnedMeshRenderer>().material = clothMat;

                    ad.transform.position = pos;
                    ad.transform.rotation = rot;

                    return ad;
                }
            case LOD.FullSize_Diffuse_BasicAnim:
                {
                    int idx = _characterIdxShuffledList[(_nextCharacterIdx++) % characterNum];
                    GameObject adObj = new GameObject("Lv " + (int)lod + " agent", typeof(Audience));
                    Animator body = Instantiate(fullSizeBody[idx]) as Animator;
                    body.transform.parent = adObj.transform;
                    body.transform.localPosition = asseblingPos;
                    body.transform.localRotation = asseblingRot;

                    Audience ad = adObj.GetComponent<Audience>();
                    adObj.name += " " + ad.agentId;
                    ad.headTransform = (from x in ad.transform.GetComponentsInChildren<Transform>()
                                        where x.gameObject.name == "Head_wrapper" || x.gameObject.name == "HeadWrapper"
                                        select x).FirstOrDefault();
                    ad.detailLevel = lod;

                    adObj.AddComponent(typeof(AudienceAnimHandlerBasic)); //template version will crash Unity...
                    var handler = adObj.GetComponent<AudienceAnimHandlerBasic>();
                    handler.controller = body;

                    var icon = Instantiate<GameObject>(prefabEyeIcon) as GameObject;
                    icon.transform.parent = ad.headTransform;
                    icon.SetActive(false);
                    icon.transform.localPosition = AudienceAnimHandlerBasic.eyeIconOffset;
                    ad.GetComponent<AudienceAnimHandlerBasic>().eyeIcon = icon;

                    Material clothMat, skinMat;
                    GetAudienceMat(false, idx, out clothMat, out skinMat);
                    body.transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material = skinMat;
                    body.transform.Find("Shoes").GetComponent<SkinnedMeshRenderer>().material = clothMat;

                    ad.transform.position = pos;
                    ad.transform.rotation = rot;

                    return ad;
                }
            case LOD.HalfSize_Diffuse_BasicAnim: default:
                {
                    int idx = _characterIdxShuffledList[(_nextCharacterIdx++) % characterNum];
                    GameObject adObj = new GameObject("Lv " + (int)lod + " agent", typeof(Audience));
                    Animator body = Instantiate(halfSizeBody[idx]) as Animator;
                    body.transform.parent = adObj.transform;
                    body.transform.localPosition = asseblingPos;
                    body.transform.localRotation = asseblingRot;

                    Audience ad = adObj.GetComponent<Audience>();
                    adObj.name += " " + ad.agentId;
                    ad.headTransform = (from x in ad.transform.GetComponentsInChildren<Transform>()
                                        where x.gameObject.name == "Head_wrapper" || x.gameObject.name == "HeadWrapper"
                                        select x).FirstOrDefault();
                    ad.detailLevel = lod;

                    adObj.AddComponent(typeof(AudienceAnimHandlerBasic)); //template version will crash Unity...
                    var handler = adObj.GetComponent<AudienceAnimHandlerBasic>();
                    handler.controller = body;

                    var icon = Instantiate<GameObject>(prefabEyeIcon) as GameObject;
                    icon.SetActive(false);
                    icon.transform.parent = ad.headTransform;
                    icon.transform.localPosition = AudienceAnimHandlerBasic.eyeIconOffset;
                    ad.GetComponent<AudienceAnimHandlerBasic>().eyeIcon = icon;

                    Material clothMat, skinMat;
                    GetAudienceMat(false, idx, out clothMat, out skinMat);
                    body.transform.Find("Body").GetComponent<SkinnedMeshRenderer>().material = skinMat;
                    body.transform.Find("Shoes").GetComponent<SkinnedMeshRenderer>().material = clothMat;

                    ad.transform.position = pos;
                    ad.transform.rotation = rot;

                    return ad;
                }
        }
    }

    private void GetAudienceMat(bool diffuseOrBump, int idx, out Material clothMat, out Material skinMat)
    {
        if (audienceClothMaterials == null || audienceClothMaterials.Length == 0)
            audienceClothMaterials = new Material[2 * characterNum];

        if (audienceClothMaterials[(diffuseOrBump ? 1 : 0) * characterNum + idx] == null)
        {
            int offset = diffuseOrBump ? 1 : 0 * characterNum + idx;
            if (diffuseOrBump)
            {
                var mat = new Material(bumpShader);
                mat.name = string.Format("mat_cloth{0}", offset);
                mat.mainTexture = audienceClothAlbedos[idx];
                mat.SetTexture("_BumpMap", audienceClothNrms[idx]);
                audienceClothMaterials[offset] = mat;
            }
            else
            {
                var mat = new Material(diffuseShader);
                mat.name = string.Format("mat_cloth{0}", offset);
                mat.mainTexture = audienceClothAlbedos[idx];
                audienceClothMaterials[offset] = mat;
            }
        }

        clothMat = audienceClothMaterials[(diffuseOrBump ? 1 : 0) * characterNum + idx];


        if (audienceSkinMaterials == null || audienceSkinMaterials.Length == 0)
            audienceSkinMaterials = new Material[2 * characterNum];

        if (audienceSkinMaterials[(diffuseOrBump ? 1 : 0) * characterNum + idx] == null)
        {
            int offset = diffuseOrBump ? 1 : 0 * characterNum + idx;
            if (diffuseOrBump)
            {
                var mat = new Material(bumpShader);
                mat.name = string.Format("mat_skin{0}", offset);
                mat.mainTexture = audienceSkinAlbedos[idx];
                mat.SetTexture("_BumpMap", audienceSkinNrms[idx]);
                audienceSkinMaterials[offset] = mat;
            }
            else
            {
                var mat = new Material(diffuseShader);
                mat.name = string.Format("mat_skin{0}", offset);
                mat.mainTexture = audienceSkinAlbedos[idx];
                audienceSkinMaterials[offset] = mat;
            }
        }

        skinMat = audienceSkinMaterials[(diffuseOrBump ? 1 : 0) * characterNum + idx];
    }
}
