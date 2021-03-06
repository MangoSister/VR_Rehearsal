﻿/*
 * Parsing seatings rotatiation and position from Unity 
 * updated : 2/6 2016
 * created by Byunghwan Lee, modified by Yang (made it a static class)
 */
using UnityEngine;
using System;
using System.IO;
using System.Collections;

public static class spaceInfoParser
{

	public struct parsedData_spaceInfo{
		public int seat_RowNum;
		public int seat_ColNum;
		public Vector3[] seat_posVecs;
		public Quaternion[] seat_rotQuans;

	}

	public static parsedData_spaceInfo Parse(string mapName){

		int numOfSeat;
		parsedData_spaceInfo parsedData = new parsedData_spaceInfo ();
		TextAsset obj = Resources.Load("MapInfo/" + mapName) as TextAsset;
		Stream s = new MemoryStream(obj.bytes);

		using(var r = new BinaryReader(s)){
			// 1. Retrieving Seats position and rotations
			parsedData.seat_RowNum = r.ReadInt32();
			parsedData.seat_ColNum = r.ReadInt32();
			
			numOfSeat = parsedData.seat_RowNum * parsedData.seat_ColNum;
			
			parsedData.seat_posVecs = new Vector3[numOfSeat];
			parsedData.seat_rotQuans = new Quaternion[numOfSeat];
			
			for(int i =0; i < numOfSeat; ++i){
				
				parsedData.seat_posVecs[i].x = r.ReadSingle();
				parsedData.seat_posVecs[i].y = r.ReadSingle();
				parsedData.seat_posVecs[i].z = r.ReadSingle();

				parsedData.seat_rotQuans[i].w = r.ReadSingle();
				parsedData.seat_rotQuans[i].x = r.ReadSingle();
				parsedData.seat_rotQuans[i].y = r.ReadSingle();
				parsedData.seat_rotQuans[i].z = r.ReadSingle();
				
			}
			
		}
#if UNITY_EDITOR
        Debug.Log("Load complete");
#endif
        Resources.UnloadAsset(obj);
        return parsedData;

	}
	
}
