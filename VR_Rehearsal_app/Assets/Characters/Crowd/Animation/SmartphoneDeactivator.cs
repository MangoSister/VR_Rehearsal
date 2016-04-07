using UnityEngine;
using System.Collections;

public class SmartphoneDeactivator : StateMachineBehaviour
{

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        (animator.transform.parent.GetComponent<Audience>().animHandler as AudienceAnimHandlerFull).smartPhone.SetActive(false);
    }
}
