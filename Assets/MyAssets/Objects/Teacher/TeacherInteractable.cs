using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherInteractable : Interactable {

	public ClassManager manager { get => ClassManager.instance; }
	public bool is_class_dismissed { get; private set; }
	public event System.Action on_point_timer_over;

	[SerializeField]
	private float base_time_between_points = 10f;
	[SerializeField]
	private float initial_point_timer_target = 10f;

	private bool is_point_timer_paused = false;
	private float between_points_timer;
	private float target_points_time;
	private bool can_request_more_time;
	private ClassData data;

	private void Awake() {
		target_points_time = initial_point_timer_target;
	}

	private void calculate_time_till_next_point(float base_offset) {
		float random_offset = Random.Range(5f, 10f);
		float seconds = base_time_between_points + base_offset + random_offset;
		target_points_time = seconds;
		Debug.Log($"Wait time set to {target_points_time}s");
	}

	public void load_class_data() {
		data = manager.minigame.data;
		calculate_time_till_next_point(0f);
	}

	public void dismiss_class() {
		Debug.Log("Class dismissed");
		is_class_dismissed = true;
		can_request_more_time = false;
	}

	public void start_timer_for_next_point() {
		calculate_time_till_next_point(10f);
		can_request_more_time = true;
	}

	private void Update() {
		if (is_class_dismissed) {
			return;
		}

		if (!is_point_timer_paused) {
			tick_timer();
		}
	}

	private void tick_timer() {
		between_points_timer += Time.deltaTime;
		if (between_points_timer > target_points_time) {
			between_points_timer -= target_points_time;
			on_point_timer_over?.Invoke();
		}
	}

	public override bool is_active() {
		return !is_class_dismissed && can_request_more_time && !manager.minigame.is_playing_minigame;
	}
	public override void do_interaction(Interactor interactor) {
		is_point_timer_paused = true;
		StartCoroutine(extend_timer());
	}

	private IEnumerator extend_timer() {
		can_request_more_time = false;
		float timer = target_points_time - between_points_timer;
		float extra_time = Random.Range(20f, 50f);

		if (timer < 8f) extra_time += Random.Range(10f, 20f);
		if (timer < 30f) extra_time += Random.Range(5f, 10f);

		target_points_time += extra_time;

		yield return new WaitForSeconds(1);
		is_point_timer_paused = false;
	}

	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.GetComponent<PlayerObject>();
		return player_obj != null;
	}
}
