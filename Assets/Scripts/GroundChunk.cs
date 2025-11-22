using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GroundChunk : MonoBehaviour {
  public Sprite leftTile;
  public Sprite middleTile;
  public Sprite rightTile;

  public float tileWidth = 1f;
  public string sortingLayerName = "Foreground";
  public int sortingOrder = 0;

  private Transform _tileContainer;
  private int _currentLength = 0;
  private readonly List<GameObject> _spawnedObstacles = new();

  private void Awake() {
    _tileContainer = transform.Find("Tiles");
  }

  private void OnDisable() {
    _currentLength = 0;
  }

  public void Build(int length, List<GameObject> obstaclePool, float obstacleChance, bool spawnObstacles = true) {
    // Recycle previous obstacles
    RecycleObstacles(obstaclePool);

    _currentLength = length;

    // Ensure tile container
    if (_tileContainer == null) _tileContainer = transform.Find("Tiles");

    // Collect existing tile children for reuse
    var existing = new List<Transform>();
    for (int i = 0; i < _tileContainer.childCount; i++) existing.Add(_tileContainer.GetChild(i));

    // Ensure we have exactly 'length' active tile objects (reuse where possible)
    for (int i = 0; i < length; i++) {
      GameObject tileObj;
      SpriteRenderer sr;
      if (i < existing.Count) {
        tileObj = existing[i].gameObject;
        tileObj.SetActive(true);
        sr = tileObj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = tileObj.AddComponent<SpriteRenderer>();
      }
      else {
        tileObj = new GameObject("Tile");
        tileObj.transform.SetParent(_tileContainer);
        sr = tileObj.AddComponent<SpriteRenderer>();
      }

      // set sprite and ordering
      if (i == 0) sr.sprite = leftTile;
      else if (i == length - 1) sr.sprite = rightTile;
      else sr.sprite = middleTile;
      sr.sortingLayerName = sortingLayerName;
      sr.sortingOrder = sortingOrder;

      tileObj.transform.localPosition = new Vector3(i * tileWidth, 0f, 0f);
    }

    // Disable any extra tiles left over from previous larger length
    for (int i = length; i < existing.Count; i++) {
      existing[i].gameObject.SetActive(false);
    }

    // Adjust collider
    var col = GetComponent<BoxCollider2D>();
    if (col) {
      col.size = new Vector2(length * tileWidth, col.size.y);
      col.offset = new Vector2(length * tileWidth / 2f, col.offset.y);
    }

    // Spawn obstacles only if allowed
    if (!spawnObstacles) return;

    // Obstacle spawning logic (max 1 per chunk)
    int maxObstacles = 1;
    int obstaclesPlaced = 0;

    List<int> tileIndices = new();
    for (int i = 0; i < length; i++) tileIndices.Add(i);
    for (int i = 0; i < tileIndices.Count; i++) {
      int j = Random.Range(i, tileIndices.Count);
      (tileIndices[j], tileIndices[i]) = (tileIndices[i], tileIndices[j]);
    }

    foreach (int i in tileIndices) {
      if (obstaclesPlaced >= maxObstacles) break;
      if (Random.value > obstacleChance) continue;

      GameObject obstacle = GetPooledObstacle(obstaclePool);
      if (obstacle == null) continue;

      obstacle.transform.SetParent(_tileContainer);
      var renderer = obstacle.GetComponent<SpriteRenderer>();
      obstacle.transform.localPosition = new Vector3(i * tileWidth, renderer.bounds.size.y, 0f);
      obstacle.SetActive(true);
      _spawnedObstacles.Add(obstacle);

      obstaclesPlaced++;
    }
  }

  private void CreateTile(Sprite sprite, int index) {
    GameObject tileObj = new("Tile");
    tileObj.transform.SetParent(_tileContainer);
    tileObj.transform.localPosition = new Vector3(index * tileWidth, 0f, 0f);

    SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>();
    sr.sprite = sprite;
    sr.sortingLayerName = sortingLayerName;
    sr.sortingOrder = sortingOrder;
  }

  private GameObject GetPooledObstacle(List<GameObject> pool) {
    foreach (var obj in pool) {
      if (!obj.activeSelf) return obj;
    }
    return null;
  }

  public void RecycleObstacles(List<GameObject> pool) {
    foreach (var obj in _spawnedObstacles) {
      obj.SetActive(false);
      obj.transform.SetParent(null);
    }
    _spawnedObstacles.Clear();
  }

  public float GetLength() => _currentLength * tileWidth;
}

