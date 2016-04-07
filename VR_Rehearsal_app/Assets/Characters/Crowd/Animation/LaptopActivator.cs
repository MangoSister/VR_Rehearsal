using UnityEngine;
using System.Collections;

public class LaptopActivator : StateMachineBehaviour
{
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        (animator.transform.parent.GetComponent<Audience>().animHandler as AudienceAnimHandlerFull).AnimateLaptop(true);
        
    }
    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        (animator.transform.parent.GetComponent<Audience>().animHandler as AudienceAnimHandlerFull).AnimateLaptop(false);
    }

}
