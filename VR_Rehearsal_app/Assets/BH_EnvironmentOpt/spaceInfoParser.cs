/*
 * Parsing seatings rotatiation and position from Unity 
 * updated : 2/4 2016
 * by Byunghwan Lee
 */
using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class spaceInfoParser : MonoBehaviour {

	public GameObject[] audience;
	public string namp;

	public struct parsedData_spaceInfo{
		public int seat_RowNum;
		public int seat_ColNum;
		public Vector3[] seat_posVecs;
		public Vector3[] seat_rotVecs;

	}


	void Awake(){
		parsedData_spaceInfo tx = Parse (namp);
		Debug.Log("ddd");

		for(int i =0 ; i<tx.seat_RowNum * tx.seat_ColNum ; i++){
			int rand = UnityEngine.Random.Range(0, (audience.Length - 1 ));
			Instantiate(audience[rand], tx.seat_posVecs[i] , Quaternion.identity);

		}
	}


	parsedData_spaceInfo Parse(string mapName){
		
		#if UNITY_EDITOR
		string loadPath = Application.dataPath + "/BH_EnvironmentOpt/EnvroInfo/" + mapName + ".bhm";
		#elif UNITY_ANDROID 
		string loadPath = Application.persistentDataPath + "/BH_EnvironmentOpt/EnvroInfo/" + mapName + ".bhm";
		#endif
		
		int numOfSeat;
		parsedData_spaceInfo parsedData = new parsedData_spaceInfo ();

		using(var r = new BinaryReader(File.OpenRead(loadPath))){

			// 1. Retrieving Seats position and rotations
			parsedData.seat_RowNum = r.ReadInt32();
			parsedData.seat_ColNum = r.ReadInt32();

			numOfSeat = parsedData.seat_RowNum * parsedData.seat_ColNum;

			parsedData.seat_posVecs = new Vector3[numOfSeat];
			parsedData.seat_rotVecs = new Vector3[numOfSeat];

			for(int i =0; i < numOfSeat; ++i){
			
				parsedData.seat_posVecs[i].x = r.ReadSingle();
				parsedData.seat_posVecs[i].y = r.ReadSingle();
				parsedData.seat_posVecs[i].z = r.ReadSingle();
				
				parsedData.seat_rotVecs[i].x = r.ReadSingle();
				parsedData.seat_rotVecs[i].y = r.ReadSingle();
				parsedData.seat_rotVecs[i].z = r.ReadSingle();

			}
			
		}
		Debug.Log("Load complete");
		return parsedData;
	}
	
}
