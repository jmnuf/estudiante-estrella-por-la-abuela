using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", order = 1)]
public class BabelPage : ScriptableObject {
	[SerializeField]
	private BabelPagePiece[] page_pieces;
	public BabelPageContent translations = new BabelPageContent();

	private void OnValidate() {
		translations = new BabelPageContent();
		foreach (var piece in page_pieces) {
			translations.TryAdd(piece.language, piece.content);
		}
	}

	public string this[BabelLanguage language] {
		get { return translations[language]; }
	}

	public string current_translation() {
		var current_language = BabelStone.get_current_language();
		return translations[current_language];
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

