using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTeacher : Interactable {
	private int important_point;
	[SerializeField]
	[Range(5, 10)]
	private int max_important_points = 5;
	private bool suggesting_note_taking;

	void Start() {
		important_point = 0;
		StartCoroutine(wait_till_next_important_point(0));
	}

	IEnumerator wait_till_next_important_point(int base_offset) {
		// range between 0 - 10 inclusive
		int random_offset = Random.Range(-1, 10) + 1;
		int seconds = 1 + base_offset + random_offset;
		
		Debug.Log("Next important point in " + seconds + "s");

		yield return new WaitForSeconds(seconds);

		important_point += 1;
		suggesting_note_taking = true;
	}

	public override bool is_active() {
		return suggesting_note_taking;
	}

	public override void do_interaction(Interactor interactor) {
		Debug.Log("Allow taking of notes");
		suggesting_note_taking = false;
		if (important_point >= max_important_points) {
			Debug.Log("Class is over");
			return;
		}

		StartCoroutine(wait_till_next_important_point(2));
	}
	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.gameObject.GetComponent<PlayerObject>();
		return player_obj != null;
	}
	public override string get_interaction_text() {
		return "request time";
	}
}
