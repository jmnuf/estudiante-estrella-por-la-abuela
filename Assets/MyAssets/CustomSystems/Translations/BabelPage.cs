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

