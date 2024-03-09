using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTeacher : Interactable {
	private int important_point;
	[SerializeField]
	[Range(5, 10)]
	private int max_important_points = 5;
	private bool suggesting_note_taking;
	private float timer;
	private float timer_target;
	[SerializeField]
	private float base_time_between_points = 10;
	private bool class_dismissed = false;

	public delegate void NewImportantPointCalledCallback(int point, int max_points);
	public delegate void InteractionCallback();
	public delegate void ClassDismissedCallback();

	public NewImportantPointCalledCallback new_important_point_called;
	public InteractionCallback on_interaction;
	public ClassDismissedCallback on_class_dismissed;

	void Start() {
		important_point = 0;
		calculate_time_till_next_point(0f);
	}

	void declare_important_point() {
		important_point += 1;
		suggesting_note_taking = true;
		new_important_point_called?.Invoke(important_point, max_important_points);
	}

	void calculate_time_till_next_point(float base_offset) {
		float random_offset = 1 + Random.Range(-1f, 10f);
		float seconds = base_time_between_points + base_offset + random_offset;
		timer_target = seconds;
		Debug.Log("Wait time set to " + timer_target + "s");
	}

	void Update() {
		if (class_dismissed) {
			return;
		}

		timer += Time.deltaTime;

		if (timer > timer_target) {
			timer -= timer_target;
			if (important_point >= max_important_points) {
				Debug.Log("Class is dismissed");
				class_dismissed = true;
				on_class_dismissed?.Invoke();
				return;
			}
			declare_important_point();
			calculate_time_till_next_point(1f);
		}
	}

	public void increase_time_till_next_point(float extra_time) {
		timer_target += extra_time;
		Debug.Log("Wait time increased and now is " + timer_target + "s");
	}

	public int get_max_points() {
		return max_important_points;
	}

	public override bool is_active() {
		return suggesting_note_taking && !class_dismissed;
	}

	public override void do_interaction(Interactor interactor) {
		suggesting_note_taking = false;
		on_interaction?.Invoke();
		float extra_time = 10f + Random.Range(1f, 11f);
		if (timer < 10f) extra_time += 10f;
		if (timer > 25f) extra_time -= 5f;
		increase_time_till_next_point(extra_time);
	}
	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.gameObject.GetComponent<PlayerObject>();
		return player_obj != null;
	}

	public override string get_interaction_text() {
		return "request time";
	}
}
