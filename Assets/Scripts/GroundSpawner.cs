using System.Collections.Generic;
using UnityEngine;

public class GroundSpawner : MonoBehaviour {
	[Header("Tileset")]
	[SerializeField] private GameObject groundChunkPrefab;
	[SerializeField] private Sprite leftTile;
	[SerializeField] private Sprite middleTile;
	[SerializeField] private Sprite rightTile;

	[Header("Spawner Settings")]
	[SerializeField] private Transform player;
	[SerializeField] private int poolSize = 20;
	[SerializeField] private Vector2 chunkLengthRange = new(4, 12);
	[SerializeField] private float despawnDistance = 30f;
	[SerializeField] private float spawnAheadDistance = 50f;

	[Header("Obstacles")]
	[SerializeField] private GameObject[] obstaclePrefabs;
	[SerializeField, Range(0f, 1f)] private float obstacleChance = 0.3f;
	[SerializeField] private int obstaclePoolSize = 50;

	private readonly List<GameObject> _activeChunks = new();
	private readonly Queue<GameObject> _inactiveChunks = new();
	private readonly List<GameObject> _obstaclePool = new();

	private float _nextSpawnX = 0f;
	private float _spawnY = 0f;

	private void Start() {
		// Create chunk pool
		for (int i = 0; i < poolSize; i++) {
			GameObject obj = Instantiate(groundChunkPrefab, Vector3.one * 1000, Quaternion.identity);
			obj.SetActive(false);

			GroundChunk gc = obj.GetComponent<GroundChunk>();
			gc.leftTile = leftTile;
			gc.middleTile = middleTile;
			gc.rightTile = rightTile;

			_inactiveChunks.Enqueue(obj);
		}

		// Create obstacle pool
		for (int i = 0; i < obstaclePoolSize; i++) {
			if (obstaclePrefabs.Length == 0) break;
			GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
			GameObject obj = Instantiate(prefab, Vector3.one * 1000, Quaternion.identity);
			obj.SetActive(false);
			_obstaclePool.Add(obj);
		}

		SpawnInitialChunk();
	}

	private void Update() {
		// Recycle old chunks
		for (int i = _activeChunks.Count - 1; i >= 0; i--) {
			GameObject chunkObj = _activeChunks[i];
			GroundChunk chunk = chunkObj.GetComponent<GroundChunk>();
			float cameraLeft = Camera.main.transform.position.x - Camera.main.orthographicSize * Camera.main.aspect;

			if (chunkObj.transform.position.x + chunk.GetLength() < cameraLeft - despawnDistance) {
				chunk.RecycleObstacles(_obstaclePool);
				chunkObj.SetActive(false);
				chunkObj.transform.position = Vector3.one * 1000;
				_inactiveChunks.Enqueue(chunkObj);
				Debug.Log("Chunk recycled. Pool size: " + _inactiveChunks.Count);
				_activeChunks.RemoveAt(i);

				if (_activeChunks.Count == 0) {
					_nextSpawnX = player.position.x;
				}
			}
		}

		// Spawn new chunks ahead using the current rightmost edge as an anchor
		float cameraRight = Camera.main.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect;
		int safetyCounter = 100;

		// limit spawns per frame to avoid expensive burst work
		int spawnedThisFrame = 0;
		const int maxSpawnsPerFrame = 3; // lower this if freeze persists

		// Keep spawning while the rightmost edge is behind the desired spawn boundary
		while (GetRightmostEdge() < cameraRight + spawnAheadDistance && safetyCounter-- > 0) {
			if (spawnedThisFrame++ >= maxSpawnsPerFrame) {
				Debug.Log("Spawn cap reached this frame, deferring remaining spawns.");
				break;
			}

			if (!SpawnChunkAt(GetRightmostEdge())) {
				Debug.LogWarning("Stopped spawning: no inactive chunks available.");
				break;
			}
		}

		if (safetyCounter <= 0) {
			Debug.LogError("Safety counter reached zero in GroundSpawner.Update(). Possible infinite loop?");
		}
	}

	private void SpawnInitialChunk() {
		GameObject chunkObj = GetInactiveChunk();
		if (chunkObj == null) return;

		GroundChunk chunk = chunkObj.GetComponent<GroundChunk>();

		chunk.Build(12, _obstaclePool, obstacleChance, spawnObstacles: false);

		_spawnY = player.position.y - 1f;
		chunkObj.transform.position = new Vector3(-5f, _spawnY, 0f);
		chunkObj.SetActive(true);

		_nextSpawnX = chunkObj.transform.position.x + chunk.GetLength() + 1;
		_activeChunks.Add(chunkObj);
	}

	private bool SpawnChunk() {
		// keep compatibility: spawn relative to stored _nextSpawnX if needed
		return SpawnChunkAt(_nextSpawnX - 1f); // place after that anchor
	}

	// spawn a chunk anchored at the given right-edge (anchorX). new chunk will be placed at anchorX + 1
	private bool SpawnChunkAt(float anchorX) {
		GameObject chunkObj = GetInactiveChunk();
		if (chunkObj == null) return false; // indicate failure to spawn

		GroundChunk chunk = chunkObj.GetComponent<GroundChunk>();

		int length = Random.Range((int)chunkLengthRange.x, (int)chunkLengthRange.y);
		chunk.Build(length, _obstaclePool, obstacleChance);

		if (player.gameObject.GetComponent<PlayerController>().IsGrounded)
			_spawnY = Random.Range(player.position.y - 1.5f, player.position.y + 0.5f);
		else
			_spawnY = player.position.y;

		// place the new chunk immediately after the anchor edge
		float placeX = anchorX + 1f;
		chunkObj.transform.position = new Vector3(placeX, _spawnY, 0f);
		chunkObj.SetActive(true);

		_activeChunks.Add(chunkObj);

		// keep _nextSpawnX as a fallback/snapshot
		_nextSpawnX = placeX + chunk.GetLength() + 1f;

		return true;
	}

	// returns the current rightmost edge X of active chunks, or player.x / _nextSpawnX fallback if none
	private float GetRightmostEdge() {
		if (_activeChunks.Count == 0) return _nextSpawnX != 0f ? _nextSpawnX : player.position.x;
		float right = float.MinValue;
		foreach (var obj in _activeChunks) {
			GroundChunk c = obj.GetComponent<GroundChunk>();
			float edge = obj.transform.position.x + c.GetLength();
			if (edge > right) right = edge;
		}
		return right;
	}

	private GameObject GetInactiveChunk() {
		if (_inactiveChunks.Count > 0)
			return _inactiveChunks.Dequeue();

		// Fallback: instantiate an extra chunk (warn and return it) — this avoids silent infinite spawn attempts
		Debug.LogWarning("No inactive chunks available in pool — instantiating a fallback chunk.");
		var obj = Instantiate(groundChunkPrefab, Vector3.one * 1000, Quaternion.identity);
		obj.SetActive(false);
		GroundChunk gc = obj.GetComponent<GroundChunk>();
		gc.leftTile = leftTile;
		gc.middleTile = middleTile;
		gc.rightTile = rightTile;
		return obj;
	}
}

