using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteractor : Interactor {
	// [RequireComponent(typeof(TextMeshProUGUI))]
	public GameObject interaction_text_label;

	public void attempt_to_interact(FPSCurrentRotation cur_rotation) {
		var interactable = check_for_interactions(cur_rotation);
		if (interactable == null) {
			set_interaction_text_label_active_state(false);
			return;
		}

		if (interaction_text_label != null) {
			interaction_text_label.SetActive(true);
			var text_mesh = interaction_text_label.GetComponent<TextMeshProUGUI>();
			text_mesh.text = get_interaction_text(interactable);
		}

		if (!Input.GetKey(KeyCode.E)) {
			return;
		}
		interact(interactable);
	}
	
	private void set_interaction_text_label_active_state(bool state) {
		if (interaction_text_label == null) {
			return;
		}
		interaction_text_label.SetActive(state);
	}

	private string get_interaction_text(Interactable interactable) {
		return "Press \"E\" to " + interactable.get_interaction_text();
	}
}
