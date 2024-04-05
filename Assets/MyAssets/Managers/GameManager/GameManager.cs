using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public GameManager instance { get; private set; }

	private void Awake() {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(this.gameObject);
	}

	public void load_scene(GameScene scene) {
		switch (scene) {
			case GameScene.MainMenu:
				SceneManager.LoadScene(0);
				break;
			case GameScene.Classroom:
				SceneManager.LoadScene(1);
				break;
		}
	}

	public void load_classroom_scene() {
		load_scene(GameScene.Classroom);
	}

	public void quit_game() {
		Debug.Log("Quit");
		Application.Quit();
	}
}

public enum GameScene {
	MainMenu,
	Classroom,
}
