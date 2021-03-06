﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PW.Core;

namespace PW
{
	public enum PWChunkLoadPatternMode
	{
		CUBIC,
		// PRIORITY_CUBIC,
		// PRIORITY_CIRCLE,
	}

	[System.Serializable]
	public abstract class PWTerrainBase : MonoBehaviour {
		public Vector3					position;
		public int						renderDistance;
		public int						chunkSize;
		public PWChunkLoadPatternMode	loadPatternMode;
		public PWNodeGraph				graph;
		public PWTerrainStorage			terrainStorage;
		public float					terrainScale = .1f; //10 cm per point
		
		[HideInInspector]
		public GameObject		terrainRoot;
		public bool				initialized {get {return graph != null && terrainRoot != null && graphOutput != null;}}
		
		[SerializeField]
		[HideInInspector]
		private PWNodeGraphOutput	graphOutput = null;

		private	int				oldSeed = 0;
		private int				WorkerGenerationId = 42;
		
		void Start () {
			InitGraph(graph);
		}

		public void InitGraph(PWNodeGraph graph)
		{
			if (graph != null)
				this.graph = graph;
			else
				return ;
			
			graph.realMode = true;
			chunkSize = graph.chunkSize;
			graphOutput = graph.outputNode as PWNodeGraphOutput;
			graph.UpdateComputeOrder();
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
			graph.ProcessGraphOnce();
		}

		public ChunkData RequestChunk(Vector3 pos, int seed)
		{
			if (seed != oldSeed)
				graph.UpdateSeed(seed);

			graph.UpdateChunkPosition(pos);
			
			graph.ProcessGraph();

			oldSeed = seed;
			//TODO: add the possibility to retreive in Terrain materializers others output.
			object firstOutput = graphOutput.inputValues.At(0);
			if (firstOutput != null)
			{
				if (firstOutput.GetType().IsSubclassOf(typeof(ChunkData)))
					return (ChunkData)firstOutput; //return the first value of output
				else
				{
					Debug.LogWarning("graph first output is not a ChunkData");
					return null;
				}
			}
			return null;
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

		public object RequestCreate(ChunkData terrainData, Vector3 pos)
		{
			var userData = OnChunkCreate(terrainData, pos);
			if (terrainStorage == null)
				return userData;
			if (terrainStorage.isLoaded(pos))
				terrainStorage[pos].userData = userData;
			else
				terrainStorage.AddChunk(pos, terrainData, userData);
			return userData;
		}

		//Generate 2D positions
		IEnumerable< Vector3 > GenerateChunkPositions()
		{
			//snap position to the nearest chunk:
			if (chunkSize > 0)
				position = PWUtils.Round(position / chunkSize) * chunkSize;
			else
				position = Vector3.zero;

			switch (loadPatternMode)
			{
				case PWChunkLoadPatternMode.CUBIC:
					Vector3 pos = position;
					for (int x = -renderDistance; x <= renderDistance; x++)
						for (int z = -renderDistance; z <= renderDistance; z++)
						{
							Vector3 chunkPos = pos + new Vector3(x * chunkSize, 0, z * chunkSize);
							yield return chunkPos;
						}
					yield break ;
			}
			yield return position;
		}
	
		//Instanciate / update ALL chunks (must be called to refresh a whole terrain)
		public void	UpdateChunks()
		{
			if (terrainStorage == null)
				return ;

			foreach (var pos in GenerateChunkPositions())
			{
				if (!terrainStorage.isLoaded(pos))
				{
					ChunkData data = RequestChunk(pos, graph.seed);
					var userChunkData = OnChunkCreate(data, pos);
					terrainStorage.AddChunk(pos, data, userChunkData);
					/*PWWorker.EnqueueTask(
						() => RequestChunk(pos, graph.seed),
						(chunkData) => {
							ChunkData data = chunkData as ChunkData;
							var userChunkData = OnChunkCreate(data, pos);
							terrainStorage.AddChunk(pos, data, userChunkData);
						},
						WorkerGenerationId
					);*/
				}
				else
				{
					var chunk = terrainStorage[pos];
					OnChunkRender(chunk.terrainData, chunk.userData, pos);
				}
			}
		}

		public void	DestroyAllChunks()
		{
			PWWorker.StopAllWorkers(WorkerGenerationId);
			if (terrainStorage == null)
				return ;
			terrainStorage.Foreach((pos, terrainData, userData) => {
				OnChunkDestroy(terrainData, userData, (Vector3)pos);
			});
			terrainStorage.Clear();
		}

		/* Utils function to simplify the downstream scripting: */

		string				PositionToChunkName(Vector3i pos)
		{
			return "chunk (" + pos.x + ", " + pos.y + ", " + pos.z + ")";
		}

		GameObject			TryFindExistingGameobjectByName(string name)
		{
			Transform t = terrainRoot.transform.Find(name);
			if (t != null)
				return t.gameObject;
			return null;
		}

		public GameObject	CreateChunkObject(Vector3 pos)
		{
			string		name = PositionToChunkName(pos);
			GameObject	ret;

			ret = TryFindExistingGameobjectByName(name);
			if (ret != null && ret.GetComponent< MeshRenderer >() == null)
				return ret;
			else if (ret != null)
				GameObject.DestroyImmediate(ret);
			
			ret = new GameObject(name);
			ret.transform.parent = terrainRoot.transform;
			ret.transform.position = pos;
			//TODO: implement Sampler* scale (step) in the scale of the object.

			return ret;
		}
	}
}