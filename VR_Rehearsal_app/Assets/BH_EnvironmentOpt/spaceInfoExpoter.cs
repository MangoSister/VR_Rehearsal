/*
 * Exporting all meshes from Unity 
 * updated : 2/4 2016
 * by Byunghwan Lee
 * 
 * - 대상 오브젝트에 대해 static 옵션을 모두 풀어줄 것 주의
 * - 이름과 Transform 데이타 필요 주의
 * 
 * 
 */


using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class spaceInfoExpoter : MonoBehaviour {

	public string mapName;
	public GameObject[] chairUnits;
	public float intervalUnit_row;
	public float intervalUnit_col;
	public int seatRowNum;
	public int seatColNum;


	public Transform[] seatTransform;


	public void Dispose(){
		seatTransform = new Transform[seatRowNum * seatColNum];

		int idx = 0;
		for (int r = 0; r <seatRowNum; r++) {
			for(int c = 0; c <seatColNum; c++){
				float rowPos = r * intervalUnit_row + this.transform.position.x ;
				float colPos = c * intervalUnit_col + this.transform.position.z ;
				
				GameObject tempInstance = (GameObject)Instantiate(chairUnits[UnityEngine.Random.Range(0,chairUnits.Length)], new Vector3( rowPos, 0, colPos), Quaternion.identity);
				tempInstance.transform.parent = this.transform;

				seatTransform[idx] = tempInstance.transform;
				idx++;
			}
		}
	}


	public void Export(){

		if (mapName.Length == 0 && seatTransform.Length == 0 && seatRowNum == 0 && seatColNum ==0 ) {
			Debug.Log("SpaceInfo Exporter :: [Error] Check name and seats transform datas.");
			return;
		}

		#if UNITY_EDITOR
		string savePath = Application.dataPath + "/BH_EnvironmentOpt/EnvroInfo/" + mapName + ".bytes";
		#elif UNITY_ANDROID 
		string savePath = Application.persistentDataPath + "MapInfo/" + mapName + ".bhm";
		#endif

		using(var w = new BinaryWriter(File.OpenWrite(savePath))){
	
			//1. Seat position Info----------------------------------
			w.Write(System.Convert.ToInt32(seatRowNum)); 
			w.Write(System.Convert.ToInt32(seatColNum)); 
			
			for(int i =0; i < seatTransform.Length; ++i){
				w.Write(seatTransform[i].position.x );
				w.Write(seatTransform[i].position.y );
				w.Write(seatTransform[i].position.z );

				w.Write(seatTransform[i].rotation.x );
				w.Write(seatTransform[i].rotation.y );
				w.Write(seatTransform[i].rotation.z );
			}

			w.Close();
		}

		Combine (mapName);
		Debug.Log("SpaceInfo Exporter :: Save complete");
	}
	// Combine meshes to a big mesh
	private  void Combine(string _strName) {
		Mesh createdMesh =  CombineMeshes_fixed ();

		if (createdMesh == null) {
			Debug.Log("SpaceInfo Exporter [ERROR]:: Failed to create combined mesh");
			return;
		}

		transform.GetComponent<MeshFilter> ().mesh = createdMesh;
		transform.gameObject.active = true;
		string savePath = "Assets/BH_EnvironmentOpt/EnvroInfo/" + _strName + ".asset";


		AssetDatabase.CreateAsset(createdMesh, savePath);
		AssetDatabase.SaveAssets();
	}
	
	private Mesh CombineMeshes_fixed() {
		MeshRenderer[] meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>(false);
		int totalVertexCount = 0;
		int totalMeshCount = 0;

		// 다 뒤져서 가져오니 걱정할 것 없음.
		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					totalVertexCount += filter.sharedMesh.vertexCount;
					totalMeshCount++;
				}
			}
		}
		
		if(totalMeshCount == 0) {
			Debug.Log("SpaceInfo Exporter :: [Error] No meshes found in children.");
			return null;
		}
		if(totalMeshCount == 1) {
			Debug.Log("SpaceInfo Exporter :: [Error] Only 1 mesh found in children.");
			return null;
		}
		if(totalVertexCount > 65535) {
			Debug.Log("SpaceInfo Exporter :: [Error] There are too many vertices to combine into 1 mesh ("+totalVertexCount+"). The max. limit is 65535");
			return null;
		}
		
		Mesh mesh = new Mesh();
		Matrix4x4 myTransform = this.gameObject.transform.worldToLocalMatrix;
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uv1s = new List<Vector2>();
		List<Vector2> uv2s = new List<Vector2>();
		Dictionary<Material, List<int>> subMeshes = new Dictionary<Material, List<int>>();
		
		if(meshRenderers != null && meshRenderers.Length > 0) {
			foreach(MeshRenderer meshRenderer in meshRenderers) {
				MeshFilter filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				if(filter != null && filter.sharedMesh != null) {
					MergeMeshInto(filter.sharedMesh, meshRenderer.sharedMaterials, myTransform * filter.transform.localToWorldMatrix, vertices, normals, uv1s, uv2s, subMeshes);
					if(filter.gameObject != this.gameObject) {
						filter.gameObject.SetActive(false);
					}
				}
			}
		}
		
		mesh.vertices = vertices.ToArray();
		if(normals.Count>0) mesh.normals = normals.ToArray();
		if(uv1s.Count>0) mesh.uv = uv1s.ToArray();
		if(uv2s.Count>0) mesh.uv2 = uv2s.ToArray();
		mesh.subMeshCount = subMeshes.Keys.Count;
		Material[] materials = new Material[subMeshes.Keys.Count];
		int mIdx = 0;
		foreach(Material m in subMeshes.Keys) {
			materials[mIdx] = m;
			mesh.SetTriangles(subMeshes[m].ToArray(), mIdx++);
		}
		
		if(meshRenderers != null && meshRenderers.Length > 0) {
			MeshRenderer meshRend = this.gameObject.GetComponent<MeshRenderer>();
			if(meshRend == null) meshRend = this.gameObject.AddComponent<MeshRenderer>();
			meshRend.sharedMaterials = materials;
			
			MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
			if(meshFilter == null) meshFilter = this.gameObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = mesh;
		}
		return mesh;
	}
	
	private static void MergeMeshInto(Mesh meshToMerge, Material[] ms, Matrix4x4 transformMatrix, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uv1s, List<Vector2> uv2s, Dictionary<Material, List<int>> subMeshes) {
		if(meshToMerge == null) return;
		int vertexOffset = vertices.Count;
		Vector3[] vs = meshToMerge.vertices;
		
		for(int i=0;i<vs.Length;i++) {
			vs[i] = transformMatrix.MultiplyPoint3x4(vs[i]);
		}
		vertices.AddRange(vs);
		
		Quaternion rotation = Quaternion.LookRotation(transformMatrix.GetColumn(2), transformMatrix.GetColumn(1));
		Vector3[] ns = meshToMerge.normals;
		if(ns!=null && ns.Length>0) {
			for(int i=0;i<ns.Length;i++) ns[i] = rotation * ns[i];
			normals.AddRange(ns);
		}
		
		Vector2[] uvs = meshToMerge.uv;
		if(uvs!=null && uvs.Length>0) uv1s.AddRange(uvs);
		uvs = meshToMerge.uv2;
		if(uvs!=null && uvs.Length>0) uv2s.AddRange(uvs);
		
		for(int i=0;i<ms.Length;i++) {
			if(i<meshToMerge.subMeshCount) {
				int[] ts = meshToMerge.GetTriangles(i);
				if(ts.Length>0) {
					if(ms[i]!=null && !subMeshes.ContainsKey(ms[i])) {
						subMeshes.Add(ms[i], new List<int>());
					}
					List<int> subMesh = subMeshes[ms[i]];
					for(int t=0;t<ts.Length;t++) {
						ts[t] += vertexOffset;
					}
					subMesh.AddRange(ts);
				}
			}
		}
	}

	public void Reset(){
		foreach (Transform child in transform) {
			child.gameObject.SetActive(true);
			foreach(Transform childOfChild in child ){
				childOfChild.gameObject.SetActive(true);
			} 

		}
	}

	public void DeleteAll(){
		int childs = transform.childCount;
		for (int i = childs - 1; i > 0; i--)
		{
			GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
		}
	}


}

