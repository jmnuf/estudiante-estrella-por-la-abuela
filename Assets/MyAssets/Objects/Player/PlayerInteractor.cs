using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteractor : Interactor {
	public static char interaction_key_char = 'e';
	public static string interaction_key_str {
		get => $"{interaction_key_char}";
	}
	public GameObject interaction_text_label;
	public BabelPage babel_base_interaction;

	// For DEBUG usage
	// Comment out for build
	private void Update() {
		if (Input.GetKeyDown(KeyCode.L)) {
			BabelLanguage cur_lang = BabelStone.get_current_language();
			var all_langs = (BabelLanguage[]) System.Enum.GetValues(typeof(BabelLanguage));
			int cur_lang_int = (int) cur_lang;
			if (cur_lang_int + 1 >= all_langs.Length) {
				BabelStone.set_current_language((BabelLanguage) 0);
			} else {
				int next_lang_int = cur_lang_int + 1;
				BabelStone.set_current_language((BabelLanguage) next_lang_int);
			}
		}

	}

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

		if (!Input.GetKeyDown(interaction_key_str)) {
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
		string base_text = babel_base_interaction.current_translation();
		base_text = base_text.Replace("{{INTERACTION_KEY}}", $"\"{interaction_key_str.ToUpper()}\"");
		return base_text + interactable.get_interaction_text();
	}
}
