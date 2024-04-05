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
		minigame.finished_point += on_finished_point;
		minigame.hide_panels();
	}

	private void Start() {
		current_class().match(
			data => minigame.data = data,
			() => {
				Debug.Log("No initial class data set");
			}
		);
	}

	private void on_finished_point(int point_index, bool completed_correctly) {
		minigame.hide_panels();
		bool has_class_data = current_class().is_some();
		if (!has_class_data) {
			return;
		}
		var class_data = current_class().unwrap();
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
		completed_classes.Add( (class_data, completed_points, progress) );
		active_class_index += 1;
		Debug.Log("A class has been completed, save progress");
		category_finished?.Invoke(class_data, completed_points, progress);
		on_category_finished?.Invoke(class_data, completed_points, progress);
		if (active_class_index < classes.Length) {
			return;
		}
		classes_completed();
	}

	private void classes_completed() {
		Debug.Log($"Classes Gauntlet completed, finished {completed_classes.Count} classes");
		gauntlet_finished?.Invoke(completed_classes.ToArray());
		on_gauntlet_finished?.Invoke(completed_classes.ToArray());
	}
}
