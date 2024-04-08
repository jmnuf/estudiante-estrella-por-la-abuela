using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClassesGauntletFinished: UnityEngine.Events.UnityEvent<(ClassData, int, float)[]> {
	public delegate void GauntletFinishedFn((ClassData, int, float)[] values);
}
[System.Serializable]
public class ClassCategoryFinished: UnityEngine.Events.UnityEvent<ClassData, int, float> {
	public delegate void CategoryFinisihedFn(ClassData data, int finished_points, float progress);
}

[RequireComponent(typeof(ClassMiniGamePromptManager))]
public class ClassManager : MonoBehaviour {
	public static ClassManager instance { get; private set; }
	public TeacherInteractable teacher;
	public NotebookInteractable notebook;
	public PlayerObject player;

	[SerializeField]
	private ClassesGauntletFinished gauntlet_finished;
	[SerializeField]
	private ClassCategoryFinished category_finished;

	public event ClassCategoryFinished.CategoryFinisihedFn on_category_finished;
	public event ClassesGauntletFinished.GauntletFinishedFn on_gauntlet_finished;

	public ClassMiniGamePromptManager minigame { get; private set; }
	public ClassData[] classes;
	private int active_class_index = 0;
	private List<(ClassData, int, float)> completed_classes = new List<(ClassData, int, float)>();

	public Option<ClassData> current_class() {
		int classes_count = classes.Length;
		if (classes_count == 0) return Option<ClassData>.None;
		if (active_class_index < 0 || active_class_index >= classes_count) {
			return Option<ClassData>.None;
		}
		ClassData data = classes[active_class_index];
		return Option<ClassData>.Some(data);
	}

	private void Awake() {
		if (instance != null) {
			if (instance.gameObject != this.gameObject) {
				Destroy(instance.gameObject);
			} else {
				Destroy(instance);
			}
			instance = null;
		}
		instance = this;

		minigame = GetComponent<ClassMiniGamePromptManager>();
		// Connect Listeners
		minigame.finished_point += on_finished_point;
		minigame.on_new_point_added += point => {
			if (!minigame.is_playing_minigame) {
				notebook.can_do_interaction = true;
			}
		};
		minigame.hide_game_panels();

		notebook.on_notebook_interaction += () => {
			FirstPersonCameraController.unlock_camera();
			notebook.can_do_interaction = false;
			minigame.show_active_point_panel();
		};

		teacher.on_point_timer_over += () => {
			Debug.Log($"Teacher's timer ended: {minigame.active_class_point_index} / {minigame.class_point_index} / {minigame.class_points_count}");
			minigame.increase_class_point();
			if (minigame.is_category_done) {
				teacher.dismiss_class();
				level_completed();
				return;
			}
			teacher.start_timer_for_next_point();
		};
	}

	private void Start() {
		current_class().match(
			data => {
				minigame.data = data;
				minigame.increase_class_point();
				teacher.enabled = true;
			},
			() => {
				teacher.enabled = false;
				Debug.Log("No initial class data set");
			}
		);
	}

	private void on_finished_point(int point_index, bool completed_correctly) {
		minigame.hide_game_panels();
		FirstPersonCameraController.lock_camera();
		bool has_class_data = current_class().is_some();
		if (!has_class_data) {
			return;
		}
		ClassData class_data = current_class().unwrap();
		if (!completed_correctly) {
			if (minigame.is_category_done) {
				level_completed();
			}
			return;
		}
		if (!minigame.is_category_done) {
			return;
		}
		level_completed();
	}

	private void level_completed() {
		var class_data = current_class().unwrap();
		int completed_points = minigame.completed_class_points;
		float prompt_progress = minigame.interrupt_current_prompt();
		float progress = ((float) completed_points) + prompt_progress;
		completed_points = (int) progress;
		completed_classes.Add( (class_data, completed_points, progress) );
		minigame.hide_game_panels();
		StartCoroutine(coroutine_level_completed(class_data, completed_points, progress));
	}

	private IEnumerator coroutine_level_completed(ClassData class_data, int completed_points, float progress) {
		Debug.Log("A class has been completed, save progress");
		FirstPersonCameraController.unlock_camera();
		category_finished?.Invoke(class_data, completed_points, progress);
		on_category_finished?.Invoke(class_data, completed_points, progress);
		yield return minigame.show_class_results(progress);
		yield return new WaitForSecondsRealtime(5);
		minigame.hide_results_panel();
		active_class_index += 1;
		if (active_class_index >= classes.Length) {
			classes_completed();
		}
	}

	private void classes_completed() {
		Debug.Log($"Classes Gauntlet completed, finished {completed_classes.Count} classes");
		gauntlet_finished?.Invoke(completed_classes.ToArray());
		on_gauntlet_finished?.Invoke(completed_classes.ToArray());
		GameManager.instance.load_main_menu();
	}
}
