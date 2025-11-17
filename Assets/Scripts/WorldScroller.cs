using UnityEngine;

public class WorldScroller : MonoBehaviour {
	[Header("Scroll Settings")]
	[SerializeField] private float scrollSpeed = 3f;
	[SerializeField] private float acceleration = 0.05f;
	[SerializeField] private float maxSpeed = 15f;

	[Header("Objects to Scroll")]
	[SerializeField] private string[] scrollTags = { "Ground" };

	private void Update() {
		// Increase scroll speed over time
		scrollSpeed += acceleration * Time.deltaTime;
		if (scrollSpeed > maxSpeed) scrollSpeed = maxSpeed;

		// Move all objects with specified tags to the left
		foreach (string tag in scrollTags) {
			GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

			foreach (GameObject obj in objects) {
				obj.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
			}
		}
	}
}