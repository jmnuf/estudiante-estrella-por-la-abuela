using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteractor : Interactor {
	public static char interaction_key_char = 'e';
	public static string interaction_key_str {
		get => $"{interaction_key_char}";
	}
	public TMP_Text interaction_text_label;
	public BabelPage babel_base_interaction;

	public void attempt_to_interact(FPSCurrentRotation cur_rotation) {
		var interactable = check_for_interactions(cur_rotation);
		if (interactable == null) {
			interaction_text_label.text = "";
			return;
		}

		interaction_text_label.text = get_interaction_text(interactable);

		if (!Input.GetKeyDown(interaction_key_str)) {
			return;
		}
		interact(interactable);
	}
	
	private string get_interaction_text(Interactable interactable) {
		string base_text = babel_base_interaction.current_translation();
		base_text = base_text.Replace("{{INTERACTION_KEY}}", $"\"{interaction_key_str.ToUpper()}\"");
		string interaction_text = "<color=\"red\">???</color>";
		try {
			interaction_text = interactable.get_interaction_text();
		} catch (System.Exception err) {
			Debug.Log($"[ERROR] {err}");
		}
		return $"{base_text.Trim()} {interaction_text.Trim()}";
	}
}
