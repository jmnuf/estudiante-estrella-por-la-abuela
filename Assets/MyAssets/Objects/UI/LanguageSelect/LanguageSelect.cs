using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LanguageSelect : MonoBehaviour {
	[SerializeField]
	private TMP_Dropdown tmp_dropdown;
	public TMP_Dropdown dropdown { get => tmp_dropdown; }
	private List<BabelLanguage> language_options = new List<BabelLanguage>();

	private void Awake() {
		dropdown.ClearOptions();
		BabelLanguage cur_lang = BabelStone.get_current_language();
		int cur_lang_index = 0;
		string[] lang_names = BabelStone.get_language_names();
		BabelLanguage[] lang_vals = (BabelLanguage[]) System.Enum.GetValues(typeof(BabelLanguage));
		for(int i = 0; i < lang_vals.Length; ++i) {
			BabelLanguage lang_val = lang_vals[i];
			language_options.Add(lang_val);
			if (lang_val == cur_lang) {
				cur_lang_index = i;
			}
		}
		tmp_dropdown.AddOptions(new List<string>(lang_names));
		tmp_dropdown.value = cur_lang_index;
	}

	public void on_dropdown_value_changed(int new_val) {
		var lang = language_options[new_val];
		BabelStone.set_current_language(lang);
	}
}
