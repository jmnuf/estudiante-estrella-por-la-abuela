using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "babel_page", order = 1)]
public class BabelPage : ScriptableObject {
	[SerializeField]
	private BabelPagePiece[] page_pieces;
	private BabelPageContent translations = new BabelPageContent();
	private bool has_loaded_translations = false;

	private void OnValidate() {
		translations = new BabelPageContent();
		foreach (var piece in page_pieces) {
			translations.TryAdd(piece.language, piece.content);
		}
		has_loaded_translations = true;
	}

	public string this[BabelLanguage language] {
		get {
			if (!has_loaded_translations) {
				OnValidate();
			}
			return translations[language];
		}
	}

	public string current_translation() {
		var current_language = BabelStone.get_current_language();
		return this[current_language];
	}

}

[System.Serializable]
public class BabelPageContent : SortedDictionary<BabelLanguage, string> {}

[System.Serializable]
public class BabelPagePiece {
	public BabelLanguage language;
	[MultilineAttribute]
	public string content;
}

public enum BabelLanguage {
	Spanish,
	English
}

