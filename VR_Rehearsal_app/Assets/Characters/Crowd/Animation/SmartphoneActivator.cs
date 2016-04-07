using UnityEngine;
using System.Collections;

public class SmartphoneActivator : StateMachineBehaviour
{
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        (animator.transform.parent.GetComponent<Audience>().animHandler as AudienceAnimHandlerFull).smartPhone.SetActive(true);
    }

}
