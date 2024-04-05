using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class BabelStone : MonoBehaviour {
	private static BabelLanguage current_language;
	private static List<BabelStone> stones = new List<BabelStone>();
	public delegate void BabelCurrentLanguageChanged();
	public static BabelCurrentLanguageChanged on_current_language_changed;

	public BabelPage translator;
	public TMP_Text text_label;

	private void Start() {
		Assert.IsNotNull(translator, "A translation is required to operate");
		if (text_label == null) {
			text_label = GetComponent<TMP_Text>();
		}
		Assert.IsNotNull(text_label, "A Text Mesh Pro text label component is required to operate");
		stones.Add(this);
	}

	private void Awake() {
		load_language_translation();
	}

	private void OnDestroy() {
		stones.Remove(this);
	}

	public void load_language_translation() {
		string translated_content = translator[current_language];
		text_label.text = translated_content;
	}

	public static BabelLanguage get_current_language() {
		return current_language;
	}
	public static void set_current_language(BabelLanguage new_language) {
		current_language = new_language;
		foreach(var mr in stones) {
			mr.load_language_translation();
		}
		on_current_language_changed();
	}

	public static BabelLanguage get_language_from_system(BabelLanguage default_language) {
		string system_language_name = System.Enum.GetName(typeof(SystemLanguage), Application.systemLanguage).ToLower();
		foreach(BabelLanguage babel_lang in System.Enum.GetValues(typeof(BabelLanguage))) {
			string babel_lang_name = System.Enum.GetName(typeof(BabelLanguage), babel_lang).ToLower();
			if (babel_lang_name == system_language_name) {
				return babel_lang;
			}
		}
		return default_language;
	}

	public static string[] get_language_names() {
		BabelLanguage[] values = (BabelLanguage[]) System.Enum.GetValues(typeof(BabelLanguage));
		var list = new List<string>();
		foreach(var lang in values) {
			string name = lang switch {
				BabelLanguage.Spanish => "Español",
				_ => System.Enum.GetName(typeof(BabelLanguage), lang),
			};
			list.Add(name);
		}
		return list.ToArray();
	}
}
