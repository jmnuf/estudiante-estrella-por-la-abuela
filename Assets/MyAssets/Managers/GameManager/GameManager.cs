using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public static GameManager instance { get; private set; }

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

	public void load_main_menu() {
		load_scene(GameScene.MainMenu);
	}

	public void load_classroom_scene() {
		load_scene(GameScene.Classroom);
	}

	public void quit_game() {
		Debug.Log("Quit");
		Application.Quit();
	}

	/*
	public TMPro.TMP_Text[] get_all_debug_labels() {
		var list = new List<GameObject>(GameObject.FindGameObjectsWithTag("DEBUG_TEXT"))
			.ConvertAll(obj => obj.GetComponent<TMPro.TMP_Text>());
		list.RemoveAll(x => x == null);
		return list.ToArray();
	}

	public TMPro.TMP_Text get_debug_label(string label_name) {
		foreach(TMPro.TMP_Text label in get_all_debug_labels()) {
			if (label.gameObject.name == label_name) {
				return label;
			}
		}
		return null;
	}
	*/
}

public enum GameScene {
	MainMenu,
	Classroom,
}
