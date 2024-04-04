using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClassesGauntletFinished: UnityEngine.Events.UnityEvent<(ClassData, int)[]> { }
[System.Serializable]
public class ClassCategoryFinished: UnityEngine.Events.UnityEvent<ClassData, int> { }

[RequireComponent(typeof(ClassMiniGamePromptManager))]
public class ClassManager : MonoBehaviour {
	public TestTeacher teacher;
	public TestNotebook notebook;
	public PlayerObject player;

	[SerializeField]
	private ClassesGauntletFinished gauntlet_finished;
	[SerializeField]
	private ClassCategoryFinished category_finished;

	public ClassMiniGamePromptManager minigame_prompt_manager { get; private set; }
	public ClassData[] classes;
	private int active_class_index = 0;
	private List<(ClassData, int)> completed_classes = new List<(ClassData, int)>();

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
		minigame_prompt_manager = GetComponent<ClassMiniGamePromptManager>();
		minigame_prompt_manager.finished_point += on_finished_point;
		minigame_prompt_manager.hide_panels();
	}

	private void Start() {
		current_class().match(
			data => minigame_prompt_manager.data = data,
			() => {
				Debug.Log("No initial class data set");
			}
		);
	}

	private void on_finished_point(int point_index, bool completed_correctly) {
		minigame_prompt_manager.hide_panels();
		bool has_class_data = current_class().is_some();
		if (!has_class_data) {
			return;
		}
		var class_data = current_class().unwrap();
		if (!completed_correctly) {
			if (minigame_prompt_manager.is_category_done) {
				level_completed();
			}
			return;
		}
		if (!minigame_prompt_manager.is_category_done) {
			return;
		}
		level_completed();
	}

	private void level_completed() {
		var class_data = current_class().unwrap();
		int completed_points = minigame_prompt_manager.completed_class_points;
		completed_classes.Add( (class_data, completed_points) );
		active_class_index += 1;
		Debug.Log("A class has been completed, save progress");
		category_finished?.Invoke(class_data, completed_points);
		if (active_class_index < classes.Length) {
			return;
		}
		classes_completed();
	}

	private void classes_completed() {
		Debug.Log($"Classes Gauntlet completed, finished {completed_classes.Count} classes");
		gauntlet_finished?.Invoke(completed_classes.ToArray());
	}
}
