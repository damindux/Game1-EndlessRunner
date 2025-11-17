using UnityEngine;

public class WorldScroller : MonoBehaviour {
	[Header("Scroll Settings")]
	[SerializeField] private float scrollSpeed = 3f;

	[Header("Objects to Scroll")]
	[SerializeField] private string[] scrollTags = { "Ground" };

	private void Update() {
		// Move all objects with specified tags to the left
		foreach (string tag in scrollTags) {
			GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

			foreach (GameObject obj in objects) {
				obj.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
			}
		}
	}
}