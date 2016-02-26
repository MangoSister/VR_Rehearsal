using UnityEngine;
using System.Collections;

public abstract class GlobalBehaviorBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
