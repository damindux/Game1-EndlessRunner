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
	[SerializeField] private int poolSize = 15;
	[SerializeField] private Vector2 chunkLengthRange = new Vector2(4, 12);
	[SerializeField] private float despawnDistance = 30f;
	[SerializeField] private float spawnAheadDistance = 50f;

	[Header("Obstacles")]
	[SerializeField] private GameObject[] obstaclePrefabs;
	[SerializeField, Range(0f, 1f)] private float obstacleChance = 0.3f;
	[SerializeField] private int obstaclePoolSize = 50;

	private List<GameObject> _activeChunks = new();
	private List<GameObject> _chunkPool = new();
	private List<GameObject> _obstaclePool = new();

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

			_chunkPool.Add(obj);
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
				_activeChunks.RemoveAt(i);
			}
		}

		// Spawn new chunks ahead
		float cameraRight = Camera.main.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect;
		while (_nextSpawnX - player.position.x < cameraRight + spawnAheadDistance) {
			SpawnChunk();
		}
	}

	private void SpawnInitialChunk() {
		GameObject chunkObj = GetInactiveChunk();
		if (chunkObj == null) return;

		GroundChunk chunk = chunkObj.GetComponent<GroundChunk>();

		chunk.Build(12, _obstaclePool, obstacleChance, spawnObstacles: false); // <- no obstacles

		_spawnY = player.position.y - 1f;
		chunkObj.transform.position = new Vector3(-5f, _spawnY, 0f);
		chunkObj.SetActive(true);

		_nextSpawnX = chunkObj.transform.position.x + chunk.GetLength();
		_activeChunks.Add(chunkObj);
	}

	private void SpawnChunk() {
		GameObject chunkObj = GetInactiveChunk();
		if (chunkObj == null) return;

		GroundChunk chunk = chunkObj.GetComponent<GroundChunk>();

		int length = Random.Range((int)chunkLengthRange.x, (int)chunkLengthRange.y);
		chunk.Build(length, _obstaclePool, obstacleChance);

		if (player.gameObject.GetComponent<PlayerController>().IsGrounded)
			_spawnY = Random.Range(player.position.y - 1.5f, player.position.y + 1.5f);
		else
			_spawnY = player.position.y;

		chunkObj.transform.position = new Vector3(_nextSpawnX, _spawnY, 0f);
		chunkObj.SetActive(true);

		_activeChunks.Add(chunkObj);
		_nextSpawnX += chunk.GetLength();
	}

	private GameObject GetInactiveChunk() {
		foreach (var obj in _chunkPool) {
			if (!obj.activeSelf) return obj;
		}
		Debug.LogWarning("No inactive chunks left in the pool!");
		return null;
	}
}

