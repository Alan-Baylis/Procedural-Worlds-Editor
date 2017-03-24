﻿using UnityEngine;

namespace PW
{
	public enum PWChunkLoadMode
	{
		CUBIC,
		// PRIORITY_CUBIC,
		// PRIORITY_CIRCLE,
	}

	[System.SerializableAttribute]
	public abstract class PWTerrainBase : MonoBehaviour {
		public Vector3			position;
		public int				viewDistance;
		public PWChunkLoadMode	loadMode;
		public PWNodeGraph		graph;
		public PWTerrainStorage	terrainStorage;
		
		public GameObject		terrainRoot;
		public bool				initialized {get {return graph != null && terrainRoot != null && graphOutput != null;}}
		
		[SerializeField]
		private PWNodeGraphOutput	graphOutput = null;

		private	int				oldSeed = 0;

		public void InitGraph(PWNodeGraph graph = null)
		{
			if (graph != null)
				this.graph = graph;
			if (graph == null)
				return ;
			graphOutput = graph.outputNode as PWNodeGraphOutput;
			if (!graph.realMode)
				terrainRoot = GameObject.Find("PWPreviewTerrain");
			if (terrainRoot == null)
			{
				terrainRoot = GameObject.Find(PWConstants.RealModeRootObjectName);
				if (terrainRoot == null)
				{
					terrainRoot = new GameObject(PWConstants.RealModeRootObjectName);
					terrainRoot.transform.position = Vector3.zero;
				}
			}
		}

		public ChunkData RequestChunk(Vector3 pos, int seed)
		{
			if (seed != oldSeed)
				graph.UpdateSeed(seed);

			graph.UpdateChunkPosition(pos);
			
			graph.ProcessGraph();

			oldSeed = seed;
			//TODO: add the possibility to retreive in Terrain materializers others output.
			//TODO: force the first output of graphOutput to be a ChunkData inherited class.
			return (ChunkData)graphOutput.inputValues.At(0); //return the first value of output
		}

		public virtual object OnChunkCreate(ChunkData terrainData, Vector3 pos)
		{
			//do nothing here, the inherited function will render it.
			return null;
		}

		public virtual void OnChunkRender(ChunkData terrainData, object userStoredObject, Vector3 pos)
		{
			//do nothing here, the inherited function will update render.
		}

		public virtual void OnChunkDestroy(ChunkData terrainData, object userStoredObject, Vector3 pos)
		{

		}

		public virtual void OnChunkHide(ChunkData terrainData, object userStoredObject, Vector3 pos)
		{

		}

		public void RequestCreate(ChunkData terrainData, Vector3 pos)
		{
			var userData = OnChunkCreate(terrainData, pos);
			if (terrainStorage == null)
				return;
			if (terrainStorage.isLoaded(pos))
				terrainStorage[pos].userData = userData;
			else
				terrainStorage.AddChunk(pos, terrainData, userData);
		}
	
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks()
		{
			//TODO: view distance loading algorithm.

			if (terrainStorage == null)
				return ;
			if (!terrainStorage.isLoaded(position))
			{
				var data = RequestChunk(position, 42);
				if (data == null)
					return ;
				var userChunkData = OnChunkCreate(data, position);
				terrainStorage.AddChunk(position, data, userChunkData);
			}
			else
			{
				var chunk = terrainStorage[position];
				OnChunkRender(chunk.terrainData, chunk.userData, position);
			}
		}

		public void OnDestroy()
		{
			Debug.Log("OMG i'm desepearing !");
		}

		public void	DestroyAllChunks()
		{
			if (terrainStorage == null)
				return ;
			terrainStorage.Foreach((pos, terrainData, userData) => {
				OnChunkDestroy(terrainData, userData, (Vector3)pos);
				terrainStorage.RemoveAt(pos);
			});
		}
	}
}