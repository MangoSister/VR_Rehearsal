using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;

public class FakeCrowdController : MonoBehaviour
{
	void Start(){
		StartCoroutine (StartWebSocket_CR ());
	}

    // Use this for initialization
    IEnumerator StartWebSocket_CR()
    {
        WebSocket w = new WebSocket(new Uri("ws://128.2.236.66:3000"));
        yield return StartCoroutine(w.Connect());

		w.SendString("Ctrl Connected");
        while (true)
        {
            string reply = w.RecvString();
            if (reply != null)
            {
                Debug.Log("Received: " + reply);
                //w.SendString("Hi there" + i++);
            }
            if (w.Error != null)
            {
                Debug.LogError("Error: " + w.Error);
                break;
            }
            yield return 0;
        }
        w.Close();
    }
}