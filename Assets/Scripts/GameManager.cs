using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour {
	public static GameManager I;

	[SerializeField] private TMP_Text scoreText;

	private float _score;

	private void Awake() {
		I = this;
	}

	private void Update() {
		AddScore();
		scoreText.text = "SCORE: " + GetScore();
	}

	private void AddScore() => _score += Time.deltaTime;

	private int GetScore() => (int)_score;
}