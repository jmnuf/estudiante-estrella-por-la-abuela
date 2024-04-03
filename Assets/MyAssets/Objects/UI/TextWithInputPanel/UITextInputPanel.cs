using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITextInputPanel : UITextPanel {
	public TMP_InputField tmpro_input_field;

	public string input_text {
		get {
			if (tmpro_input_field == null) return "";
			return tmpro_input_field.text;
		}
		set { if (tmpro_input_field != null) { tmpro_input_field.text = value; } }
	}

	public string pop_input_text() {
		string text = tmpro_input_field.text;
		tmpro_input_field.text = "";
		return text;
	}
}
