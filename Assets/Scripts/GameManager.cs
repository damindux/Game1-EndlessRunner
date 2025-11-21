using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager I;

	[SerializeField] private TMP_Text scoreText;
	[SerializeField] private GameObject gameOverScreen;

	private float _score;
	private bool _gameOver;

	private void Awake() {
		gameOverScreen.SetActive(false);
		I = this;
		Time.timeScale = 1f;
	}

	private void Update() {
		AddScore();
		scoreText.text = "SCORE: " + GetScore();
	}

	private void AddScore() => _score += Time.deltaTime;

	private int GetScore() => (int)_score;

	public void GameOver() {
		if (_gameOver) return;
		_gameOver = true;
		Time.timeScale = 0f;
		gameOverScreen.SetActive(true);
	}

	public void Restart() => SceneManager.LoadScene(0);
}