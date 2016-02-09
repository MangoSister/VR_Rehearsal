/* VRInputManager.cs
 * Yang Zhou, last modified on Feb 09, 2016
 * VRInputManager defines and manages various input events in VR presentation environment
 * Intended to be used by external listeners
 * Use RegisterEvent() and UnRegisterEvent() to subscribe/unsubscribe events
 * Dependencies: need PresenterRaycaster & Projector Screen objects in unity scene
 */

using UnityEngine;
using System.Collections;

public class VRInputManager : MonoBehaviour
{
    //convenient way to get input manager
    public static VRInputManager currTutInputManager
    { get { return FindObjectOfType<VRInputManager>(); } }

    //delegate type for input manager
    public delegate void TutorialInput_Handler();

    //define input types
    public enum TutorialInputType
    {
        NoddingHead, LookAtScreen,
    }

    //RresenterRaycaster reference (dragged it in Unity Editor)
    public PresenterRaycaster presenter;

    //projector screen reference (dragged it in Unity Editor)
    public GameObject projScreen;

    //threshold (in degrees) for detecting nodding head
    public float noddingHeadDegree = 30f;

    private TutorialInput_Handler _OnNoddingHead;
    private Coroutine _currNoddingHeadCR = null;
    private TutorialInput_Handler _OnLookAtScreen;
    private Coroutine _currLookingAtCR = null;

    //sample handler definitions
    //private TutorialInput_Handler nodFunc = () => { print("nod"); };
    //private TutorialInput_Handler lookFunc = () => { print("look"); };

    //main interface: USE ME to subscribe a sepcific type of input event
    //type: see the enum definition for different input types.
    //handler: your event handler. (will be called once the input condition is satisfied)
    public void RegisterEvent(TutorialInputType type, TutorialInput_Handler handler)
    {
        switch (type)
        {
            case TutorialInputType.NoddingHead:
                {
                    _OnNoddingHead += handler;
                    if (_currNoddingHeadCR == null && presenter != null)
                        _currNoddingHeadCR = StartCoroutine(NoddingHead_CR());
                    break;
                }
            case TutorialInputType.LookAtScreen:
                {
                    _OnLookAtScreen += handler;
                    if (_currLookingAtCR == null && presenter != null)
                        _currLookingAtCR = StartCoroutine(LookingAt_CR());
                    break;
                }
            default: break;
        }
    }

    //main interface: USE ME to unsubscribe a sepcific type of input event
    //MAKE SURE to use UnRegisterEvent IN PAIR with ResgisterEvent
    public void UnRegisterEvent(TutorialInputType type, TutorialInput_Handler handler)
    {
        switch (type)
        {
            case TutorialInputType.NoddingHead:
                {
                    _OnNoddingHead -= handler;
                    break;
                }
            case TutorialInputType.LookAtScreen:
                {
                    _OnLookAtScreen -= handler;
                    break;
                }
            default: break;
        }
    }

    private IEnumerator NoddingHead_CR()
    {
        bool touchedUp = false;
        bool touchedBottom = false;
        while (_OnNoddingHead != null)
        {
            float angle = Vector3.Angle(presenter.transform.forward, presenter.headTransform.forward);
            Vector3 cross = Vector3.Cross(presenter.transform.forward, presenter.headTransform.forward);
            if (cross.x > 0)
                angle = -angle;

            if (angle > noddingHeadDegree)
                touchedUp = true;
            if (angle < - noddingHeadDegree)
                touchedBottom = true;

            if (touchedBottom && touchedUp)
            {
                _OnNoddingHead();
                touchedUp = false;
                touchedBottom = false;
            }
            yield return null;
        }

        _currNoddingHeadCR = null;
    }

    private IEnumerator LookingAt_CR()
    {
        while (_OnLookAtScreen != null)
        {
            Ray ray = new Ray(presenter.headTransform.position, presenter.headTransform.forward);
            RaycastHit info;
            if (Physics.Raycast(ray, out info))
            {
                GameObject obj = info.collider.gameObject;
                if (obj == projScreen)
                    _OnLookAtScreen();
            }
            yield return null;
        }

        _currLookingAtCR = null;
    }

    //sample usage
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.H))
    //    {
    //        print("reg nod");
    //        RegisterEvent(TutorialInputType.NoddingHead, nodFunc);
    //    }
    //    if (Input.GetKeyDown(KeyCode.J))
    //    {
    //        print("unreg nod");
    //        UnRegisterEvent(TutorialInputType.NoddingHead, nodFunc);
    //    }
    //    if (Input.GetKeyDown(KeyCode.K))
    //    {
    //        print("reg look");
    //        RegisterEvent(TutorialInputType.LookAtScreen, lookFunc);
    //    }
    //    if (Input.GetKeyDown(KeyCode.L))
    //    {
    //        print("unreg look");
    //        UnRegisterEvent(TutorialInputType.LookAtScreen, lookFunc);
    //    }
    //}
}
