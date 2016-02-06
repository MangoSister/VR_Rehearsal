using UnityEngine;
using System.Collections;

public class GlobalBehaviorCleaner : GlobalBehaviorBase
{
    private void OnApplicationQuit()
    {
        GlobalBehaviorBase[] victims = FindObjectsOfType<GlobalBehaviorBase>();
        for (int i = 0; i < victims.Length; i++)
        {
            if (victims[i] != gameObject)
            {
                Destroy(victims[i].gameObject);
            }
        }

        Destroy(gameObject);
    }
}
