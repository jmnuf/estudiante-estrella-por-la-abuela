using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextPanel : MonoBehaviour {
	[SerializeField]
	private TMP_Text tmpro_text;
	[SerializeField]
	private BabelPage _text_page;
	public BabelPage text_page {
		get => _text_page;
		set {
			_text_page = value;
			if (value) {
				tmpro_text.text = _text_page.current_translation();
			} else {
				tmpro_text.text = string.Empty;
			}
		}
	}

	public UITextPanel() : base() {
		BabelStone.on_current_language_changed += on_babel_current_language_changed;
	}

	public void show() {
		enabled = true;
	}

	public void show_with_page(BabelPage page) {
		text_page = page;
		show();
	}

	public void hide() {
		enabled = false;
	}

	private void on_babel_current_language_changed() {
			if (_text_page != null) {
				tmpro_text.text = _text_page.current_translation();
			}
	}

	private void OnValidate() {
		text_page = _text_page;
	}
}
