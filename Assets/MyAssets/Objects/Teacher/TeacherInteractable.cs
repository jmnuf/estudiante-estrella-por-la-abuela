using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherInteractable : Interactable {

	public ClassManager manager { get => ClassManager.instance; }
	public bool is_class_dismissed { get; private set; }

	[SerializeField]
	private float base_time_between_points = 10f;

	private float between_points_timer;
	private float target_points_time;
	private bool can_request_more_time;
	private ClassData data;

	void calculate_time_till_next_point(float base_offset) {
		float random_offset = 1f + Random.Range(-1f, 10f);
		float seconds = base_time_between_points + base_offset + random_offset;
		target_points_time = seconds;
		Debug.Log($"Wait time set to {target_points_time}s");
	}

	public void load_class_data() {
		data = manager.minigame.data;
		calculate_time_till_next_point(0f);
	}

	private void Update() {
		if (is_class_dismissed) {
			return;
		}
		var minigame = manager.minigame;

		between_points_timer += Time.deltaTime;
		if (between_points_timer > target_points_time) {
			between_points_timer -= target_points_time;
			if (minigame.is_category_done) {
				Debug.Log("Class dismissed");
				is_class_dismissed = true;
				return;
			}
			minigame.increase_class_point();
			calculate_time_till_next_point(1f);
			can_request_more_time = true;
		}
	}

	public override bool is_active() {
		return !is_class_dismissed && can_request_more_time;
	}
	public override void do_interaction(Interactor interactor) {
		can_request_more_time = false;
		float timer = between_points_timer;
		float extra_time = 10f + Random.Range(0f, 10f);

		if (timer < 8f && extra_time <= 13f) extra_time += Random.Range(5f, 10f);
		if (timer > 25f) extra_time -= 10f;

		between_points_timer += extra_time;
	}
	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.GetComponent<PlayerObject>();
		return player_obj != null;
	}
}
