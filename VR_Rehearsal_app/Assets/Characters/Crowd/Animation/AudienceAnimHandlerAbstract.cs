using UnityEngine;
using System.Collections;

public abstract class AudienceAnimHandlerAbstract : MonoBehaviour
{
    public readonly static int _paramIdState = Animator.StringToHash("state");
    public readonly static int _paramIdBlendFactor0 = Animator.StringToHash("blendFactor0");
    public readonly static int _paramIdBlendFactor1 = Animator.StringToHash("blendFactor1");
    public readonly static int _paramIdSubState = Animator.StringToHash("subState");
    public readonly static int _paramIdMirror = Animator.StringToHash("mirror");

    public static Vector3 eyeIconOffset = Vector3.forward * 0.2f + Vector3.up * 0.1f;
    public static float eyeIconScale = 0.05f;
    public static float eyeIconFreq = 6f;
    public static GameObject eyeIconPrefab;

    public abstract void UpdateStateAnim();
    public abstract void UpdateChatDirection(Vector2 dir);
}
