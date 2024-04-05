using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNotebook : Interactable {
	public bool can_take_notes = false;
	public delegate void NoteTakingRequestedCallback();
	public NoteTakingRequestedCallback on_note_taking_requested;

	public override bool is_active() {
		return can_take_notes;
	}
	public override void do_interaction(Interactor interactor) {
		Debug.Log("Open up mini-game to take notes");
		can_take_notes = false;
		on_note_taking_requested?.Invoke();
	}

	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.gameObject.GetComponent<PlayerObject>();
		return player_obj != null;
	}

	new public string get_interaction_text() {
		return "take notes";
	}
}
