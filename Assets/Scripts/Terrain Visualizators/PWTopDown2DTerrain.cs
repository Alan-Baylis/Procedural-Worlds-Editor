﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using PW;
using PW.Core;

public class PWTopDown2DTerrain : PWTerrainBase {

	static Gradient			rainbow = null;

	static Mesh				topDownTerrainMesh = null;
	static int				topDownTerrainMeshSize = 0;

	void	GenerateTopDownTerrainMesh()
	{
		int					size = chunkSize * chunkSize;
		int					nFaces = (chunkSize - 1) * (chunkSize - 1);
		Vector3[]			vertices = new Vector3[size];
		Vector3[]			normals = new Vector3[size];
		Vector2[]			uvs = new Vector2[size];
		int[]				triangles = new int[nFaces * 6];

		float				terrainWidth = 1;

		topDownTerrainMesh = new Mesh();
		topDownTerrainMesh.Clear();

		for (int x = 0; x < chunkSize; x++)
		{
			float xPos = ((float)x / (chunkSize - 1) - .5f) * terrainWidth;
			for (int z = 0; z < chunkSize; z++)
			{
				float zPos = ((float)z / (chunkSize - 1) - .5f) * terrainWidth;
				vertices[z + x * chunkSize] = new Vector3(xPos, 0, zPos);
				uvs[z + x * chunkSize] = new Vector2((float)x / (chunkSize - 1), (float)z / (chunkSize - 1));
			}
		}

		for (int i = 0; i < chunkSize * chunkSize; i++)
			normals[i] = Vector3.up;

        int t = 0;
        for (int face = 0; face < nFaces; face++)
        {
            int i = face % (chunkSize - 1) + (face / (chunkSize - 1) * chunkSize);

            triangles[t++] = i + 1;
            triangles[t++] = i + chunkSize + 1;
            triangles[t++] = i + chunkSize;

            triangles[t++] = i;
            triangles[t++] = i + 1;
            triangles[t++] = i + chunkSize;
        }

        topDownTerrainMesh.vertices = vertices;
		topDownTerrainMesh.normals = normals;
		topDownTerrainMesh.triangles = triangles;
		topDownTerrainMesh.uv = uvs;
		topDownTerrainMesh.RecalculateBounds();
	}

	void					UpdateMeshDatas(BiomeMap2D biomes)
	{
		List< Vector4 >		blendInfos = new List< Vector4 >();

		for (int x = 0; x < chunkSize; x++)
			for (int z = 0; z < chunkSize; z++)
			{
				Vector4 biomeInfo = Vector4.zero;
				var biomePoint = biomes.GetBiomeBlendInfo(x, z);

				biomeInfo.x = biomePoint.firstBiomeId;
				biomeInfo.y = biomePoint.firstBiomeBlendPercent;
				biomeInfo.z = biomePoint.secondBiomeId;
				biomeInfo.w = biomePoint.secondBiomeBlendPercent;
				blendInfos.Add(biomeInfo);
			}
		topDownTerrainMesh.SetUVs(1, blendInfos);
	}
	
	public override object	OnChunkCreate(ChunkData cd, Vector3 pos)
	{
		if (cd == null)
			return null;
		
		if (rainbow == null)
			rainbow = PWUtils.CreateRainbowGradient();
		
		TopDown2DData	chunk = (TopDown2DData)cd;
		
		//create the terrain texture:
		//TODO: bind the blendMap with biome maps to the terrain shader
		//TODO: bind all vertex datas from the mesh

		GameObject g = CreateChunkObject(pos * terrainScale);
		g.transform.rotation = Quaternion.identity;
		g.transform.localScale = Vector3.one * chunkSize * terrainScale;
		
		MeshRenderer mr = g.AddComponent< MeshRenderer >();
		MeshFilter mf = g.AddComponent< MeshFilter >();
	
		if (topDownTerrainMesh == null || topDownTerrainMeshSize != chunkSize)
			GenerateTopDownTerrainMesh();
			
		UpdateMeshDatas(chunk.biomeMap);

		mf.sharedMesh = topDownTerrainMesh;

		Shader topDown2DBasicTerrainShader = Shader.Find("ProceduralWorlds/Basic terrain");
		if (topDown2DBasicTerrainShader == null)
			topDown2DBasicTerrainShader = Shader.Find("Standard");
		Material mat = new Material(topDown2DBasicTerrainShader);
		mat.SetTexture("_AlbedoMaps", chunk.albedoMaps);
		mr.sharedMaterial = mat;
		//TODO: vertex painting
		return g;
	}

	public override void 	OnChunkDestroy(ChunkData terrainData, object userStoredObject, Vector3 pos)
	{
		GameObject g = userStoredObject as GameObject;

		if (g != null)
			DestroyImmediate(g);
	}

	public override void	OnChunkRender(ChunkData cd, object chunkGameObject, Vector3 pos)
	{
		if (cd == null)
			return ;
		GameObject		g = chunkGameObject as GameObject;
		TopDown2DData	chunk = (TopDown2DData)cd;

		if (g == null) //if gameobject have been destroyed by user and reference was lost.
		{
			chunkGameObject = RequestCreate(cd, pos);
			g = chunkGameObject as GameObject;
		}
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
	}
}
