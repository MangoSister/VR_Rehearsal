using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Net;

public class FakeCrowdController : MonoBehaviour
{	
	public GameObject crowdSimObj;
	private CrowdSimulator _crowdSim;

	void Start(){

		_crowdSim = crowdSimObj.GetComponent<CrowdSimulator> ();
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
            string res = w.RecvString();
			if (res != null)
            {
				Debug.Log("Received: " + res);
                //w.SendString("Hi there" + i++);
				char[] delimiterChars = { ' ' ,'\t' };
				string[] parseStr = res.Split(delimiterChars);

				switch(parseStr[0]){
					case "m+":{
					_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.globalAttentionMean, true, parseStr);
						break;
					}
					case "m-":{
					_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.globalAttentionMean, false, parseStr);
						break;
					}
					case "d+":{
					_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.globalAttentionStDev, true, parseStr);
						break;
					}
					case "d-":{
						_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.globalAttentionStDev, false, parseStr);
						break;
					}
					case "u+":{
						_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.seatPosAttentionUpper, true, parseStr);
						break;
					}
					case "u-":{
						_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.seatPosAttentionUpper, false, parseStr);
						break;
					}
					case "l+":{
						_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.seatPosAttentionLower, true, parseStr);
						break;
					}
					case "l-":{
						_crowdSim.globalAttentionMean = SetValueFromString(_crowdSim.seatPosAttentionLower, false, parseStr);
						break;
					}

				}
			
				w.SendString("|Unity> currStat: m=" + _crowdSim.globalAttentionMean + ", dev=" + _crowdSim.globalAttentionStDev 
				             + ", A_up=" + _crowdSim.seatPosAttentionUpper + ", A_Low=" +  _crowdSim.seatPosAttentionLower + "." );

				
				//w.SendString("|Unity> blar blar" + res);

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

	float SetValueFromString( float value, bool bIsPlus, string[] str){

		float currVal = value;
		if (str.Length > 1) {
			float resNum = CheckNumberInParse (str [1]);

			if (resNum != -1) {
				if (bIsPlus){
					currVal = currVal + resNum;
				}else{
					currVal = currVal - resNum;
				}
					
			}else{
				Debug.LogError( "Wrong Number");
			}

		} else {
			if(bIsPlus){
				currVal = currVal + 0.01f;
			}else{
				currVal = currVal - 0.01f;
			}
		}

		return currVal;
	}


	float CheckNumberInParse(string _str){
		float num = float.Parse(_str);
		if (num.GetType () == typeof(float)) {
			return num;
		} else {
			return -1;
		}
	}
}