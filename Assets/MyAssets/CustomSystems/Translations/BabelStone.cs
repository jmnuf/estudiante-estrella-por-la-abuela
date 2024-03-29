using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class BabelStone : MonoBehaviour {
	private static BabelLanguage current_language;
	private static List<BabelStone> worldWides = new List<BabelStone>();

	public BabelPage translator;
	public TMP_Text text_label;

	private void Start() {
		Assert.IsNotNull(translator, "A translation is required to operate");
		if (text_label == null) {
			text_label = GetComponent<TMP_Text>();
		}
		Assert.IsNotNull(text_label, "A Text Mesh Pro text label component is required to operate");
		worldWides.Add(this);
	}

	private void Awake() {
		load_language_translation();
	}

	private void OnDestroy() {
		worldWides.Remove(this);
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
		foreach(var mr in worldWides) {
			mr.load_language_translation();
		}
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
}
