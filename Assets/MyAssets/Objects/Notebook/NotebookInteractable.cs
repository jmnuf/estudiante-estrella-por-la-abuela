using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotebookInteractable : Interactable {
	public bool can_do_interaction = false;

	public delegate void NotebookInteractionEvent();
	public event NotebookInteractionEvent on_notebook_interaction;

	public override bool is_active() {
		if (!can_do_interaction) {
			return false;
		}
		bool is_playing_minigame = ClassManager.instance.minigame.is_playing_minigame;
		return ! is_playing_minigame;
	}

	public override void do_interaction(Interactor interactor) {
		can_do_interaction = false;
		on_notebook_interaction?.Invoke();
	}

	public override bool can_interact(Interactor interactor) {
		var player_obj = interactor.GetComponent<PlayerObject>();
		return player_obj != null;
	}
}
