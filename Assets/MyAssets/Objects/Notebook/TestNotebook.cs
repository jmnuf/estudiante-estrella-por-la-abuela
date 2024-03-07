using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNotebook : Interactable {
	public override bool is_active() {
		return true;
	}
	public override void do_interaction(Interactor interactor) {
		Debug.Log("Open up mini-game to take notes");
	}
	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.gameObject.GetComponent<PlayerObject>();
		return player_obj != null;
	}
	public override string get_interaction_text() {
		return "take notes";
	}
}
