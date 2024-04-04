using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClassMiniGamePromptManager))]
public class ClassManager : MonoBehaviour {
	public TestTeacher teacher;
	public TestNotebook notebook;
	public PlayerObject player;

	public ClassMiniGamePromptManager mini_game_prompt_manager { get; private set; }
	public ClassData[] classes;
	private int active_class_index = 0;

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
		mini_game_prompt_manager = GetComponent<ClassMiniGamePromptManager>();
	}

	private void Start() {
		current_class().match<object>(
			(data) => mini_game_prompt_manager.data = data,
			() => null
		);
	}
}
