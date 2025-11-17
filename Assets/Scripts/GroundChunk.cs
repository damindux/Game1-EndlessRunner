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
  public float obstacleHeight = 1f;

  private Transform tileContainer;
  private int currentLength = 0;
  private List<GameObject> spawnedObstacles = new();

  private void Awake() {
    tileContainer = transform.Find("Tiles");
  }

  public void Build(int length, List<GameObject> obstaclePool, float obstacleChance, bool spawnObstacles = true) {
    // Clear previous tiles
    while (tileContainer.childCount > 0) {
      if (Application.isPlaying)
        Destroy(tileContainer.GetChild(0).gameObject);
      else
        DestroyImmediate(tileContainer.GetChild(0).gameObject);
    }

    // Recycle previous obstacles
    RecycleObstacles(obstaclePool);

    currentLength = length;

    // Build tiles
    CreateTile(leftTile, 0);
    for (int i = 1; i < length - 1; i++)
      CreateTile(middleTile, i);
    CreateTile(rightTile, length - 1);

    // Adjust collider
    var col = GetComponent<BoxCollider2D>();
    if (col) {
      col.size = new Vector2(length * tileWidth, col.size.y);
      col.offset = new Vector2(length * tileWidth / 2f, col.offset.y);
    }

    // Spawn obstacles only if allowed
    if (!spawnObstacles) return;

    // Obstacle spawning logic (max 2 per chunk)
    int maxObstacles = 2;
    int obstaclesPlaced = 0;

    List<int> tileIndices = new List<int>();
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

      obstacle.transform.SetParent(tileContainer);
      obstacle.transform.localPosition = new Vector3(i * tileWidth, obstacleHeight, 0f);
      obstacle.SetActive(true);
      spawnedObstacles.Add(obstacle);

      obstaclesPlaced++;
    }
  }

  private void CreateTile(Sprite sprite, int index) {
    GameObject tileObj = new GameObject("Tile");
    tileObj.transform.SetParent(tileContainer);
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
    foreach (var obj in spawnedObstacles) {
      obj.SetActive(false);
      obj.transform.SetParent(null);
    }
    spawnedObstacles.Clear();
  }

  public float GetLength() => currentLength * tileWidth;
}

