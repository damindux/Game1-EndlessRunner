using System.Collections.Generic;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
	[SerializeField] private GameObject[] groundPrefabs;
	[SerializeField] private Transform player;
	[SerializeField] private int poolSize = 10;

	private Queue<GameObject> _activeGrounds;
	private Queue<GameObject> _pool;

	private void Start()
	{
		_activeGrounds = new();
		_pool = new();

		for (int i = 0; i < poolSize; i++)
		{
			GameObject prefab = groundPrefabs[Random.Range(0, groundPrefabs.Length)];
			GameObject ground = Instantiate(prefab, Vector2.one * 100f, Quaternion.identity);
			ground.SetActive(false);
			_pool.Enqueue(ground);
		}

		InvokeRepeating(nameof(SpawnGround), 1f, 1.5f);
	}

	private void Update()
	{
		if (_activeGrounds.Count == 0) return;

		GameObject oldest = _activeGrounds.Peek();

		if (oldest.transform.position.x <= -10f)
		{
			_activeGrounds.Dequeue();
			oldest.SetActive(false);
			_pool.Enqueue(oldest);
		}
	}

	private void SpawnGround()
	{
		if (_pool.Count == 0) return;

		float y = Random.Range(player.position.y - 1.5f, player.position.y + 1.5f);

		GameObject ground = _pool.Dequeue();
		ground.transform.position = new Vector3(10f, y);
		ground.SetActive(true);

		_activeGrounds.Enqueue(ground);
	}
}