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
			if (!tmpro_text) return;
			if (value) {
				tmpro_text.text = _text_page.current_translation();
			} else {
				tmpro_text.text = string.Empty;
			}
		}
	}
	public TMP_Text tmp_text {
		get => tmpro_text;
		private set {
			tmpro_text = value;
			if (tmpro_text == null) {
				return;
			}
			if (_text_page == null) {
				tmpro_text.text = string.Empty;
				return;
			}
			tmpro_text.text = _text_page.current_translation();
		}
	}

	public UITextPanel() : base() {
		BabelStone.on_current_language_changed += on_babel_current_language_changed;
	}

	public delegate Color32 MapTextCharacterInfoToColor(int index, TMP_CharacterInfo characterInfo);
	public void color_text_letters(MapTextCharacterInfoToColor info_to_color) {
		var tmp_text_mesh = tmpro_text.gameObject.GetComponent<TextMeshProUGUI>();
		if (tmp_text_mesh == null) {
			Debug.Log("TMP_Text has no text mesh");
			return;
		}
		var text_info = tmp_text_mesh.textInfo;
		if (text_info == null) {
			Debug.Log($"Text mesh has no text info while holding text `{tmpro_text.text}`");
			return;
		}
		// Code copied from: https://forum.unity.com/threads/fixed-change-color-of-individual-characters-in-textmeshpro-text-ui.1278845/#post-8884554
		for (int i = 0; i < text_info.characterCount; ++i) {
			TMP_CharacterInfo char_info = text_info.characterInfo[i];
			// if the character is a space, the vertexIndex will be 0, which is the same as the first character. If you don't leave here, you'll keep modifying the first character's vertices, I believe this is a bug in the text mesh pro code
			if (!char_info.isVisible) {
				continue;
			}
			Color32 myColor32 = info_to_color(i, char_info);
			int meshIndex = char_info.materialReferenceIndex;
			int vertexIndex = char_info.vertexIndex;
			Color32[] vertexColors = text_info.meshInfo[meshIndex].colors32;
			vertexColors[vertexIndex + 0] = myColor32;
			vertexColors[vertexIndex + 1] = myColor32;
			vertexColors[vertexIndex + 2] = myColor32;
			vertexColors[vertexIndex + 3] = myColor32;
		}
		tmp_text_mesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
	}

	public void show() {
		gameObject.SetActive(true);
	}

	public void show_with_page(BabelPage page) {
		text_page = page;
		show();
	}

	public void hide() {
		gameObject.SetActive(false);
	}

	private void on_babel_current_language_changed() {
			if (_text_page != null && tmpro_text != null) {
				tmpro_text.text = _text_page.current_translation();
			}
	}

	private void OnValidate() {
		text_page = _text_page;
	}
}
