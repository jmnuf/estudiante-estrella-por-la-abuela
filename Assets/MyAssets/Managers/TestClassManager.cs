using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestClassManager : MonoBehaviour {
	public TestTeacher teacher;
	public TestNotebook notebook;
	public PlayerObject player;

	public GameObject prompt_panel;
	public TextMeshProUGUI text_mesh;

	public GameObject class_done_panel;
	public TextMeshProUGUI results_text_mesh;

	private bool taking_notes = false;
	private int character_index = 0;
	private int wrong_characters_count = 0;
	private string notes_string;
	private int notes_length;
	private int notes_taken = 0;

	public string[] possible_notes_to_copy;

	void Start() {
		// Setup callbacks
		teacher.new_important_point_called += on_new_important_point;
		teacher.on_interaction += on_teacher_interaction;
		teacher.on_class_dismissed += on_class_dismissed;
		notebook.on_note_taking_requested += on_notebook_note_taking_requested;

		prompt_panel.SetActive(false);
		class_done_panel.SetActive(false);
	}

	void on_new_important_point(int _point, int _max_points) {
		Debug.Log("New important point detected from the class manager");
		notebook.can_take_notes = true;
	}

	void on_teacher_interaction() {
		Debug.Log("Teacher interaction detected from the class manager");
		notebook.can_take_notes = true;
	}

	void on_class_dismissed() {
		Debug.Log("Can't take more notes as class is dismissed");

		notebook.can_take_notes = false;
		prompt_panel.SetActive(false);

		int max_points = teacher.get_max_points();
		float points_made = (float) notes_taken;
		if (taking_notes) {
			float subpoints = ((float)character_index) / ((float)notes_length);
			points_made += subpoints;
		}
		taking_notes = false;

		string results_str = "Class Finished!\n";
		results_str += points_made + " / " + max_points + " notes completed";
		results_text_mesh.text = results_str;
		class_done_panel.SetActive(true);
	}

	string get_a_note_to_copy() {
		int index = Random.Range(0, possible_notes_to_copy.Length);
		string note = possible_notes_to_copy[index];
		return note;
	}

	IEnumerator delayed_set_taking_notes(bool new_value) {
		yield return 0;
		taking_notes = new_value;
	}

	void on_notebook_note_taking_requested() {
		prompt_panel.SetActive(true);
		player.enabled = false;

		notes_string = get_a_note_to_copy();
		notes_length = notes_string.Length;

		text_mesh.text = notes_string;
		character_index = 0;
		wrong_characters_count = 0;
		StartCoroutine(delayed_set_taking_notes(true));
	}

	bool is_key_a_letter(KeyCode kc) {
		return (KeyCode.A <= kc && kc <= KeyCode.Z);
	}

	bool is_key_a_digit(KeyCode kc) {
		return (KeyCode.Keypad0 <= kc && kc <= KeyCode.Keypad9) || (KeyCode.Alpha0 <= kc && kc <= KeyCode.Alpha9);
	}

	bool is_key_a_quote(KeyCode kc) {
		return kc == KeyCode.DoubleQuote || kc == KeyCode.Quote;
	}

	bool is_key_valid(KeyCode kc) {
		if (is_key_a_letter(kc)) return true;
		if (is_key_a_digit(kc)) return true;
		if (is_key_a_quote(kc)) return true;
		switch (kc) {
			case KeyCode.LeftParen: case KeyCode.RightParen:
			case KeyCode.LeftBracket: case KeyCode.RightBracket:
			case KeyCode.Comma: case KeyCode.Period:
			case KeyCode.Minus: case KeyCode.Underscore:
			case KeyCode.Space: case KeyCode.Backspace:
				return true;
			default:
				return false;
		}
	}

	void OnGUI() {
		if (!taking_notes) return;

		Event ev = Event.current;
		if (ev.type != EventType.KeyDown) return;
		KeyCode received = ev.keyCode;
		if (!is_key_valid(received)) return;
		if (received == KeyCode.Backspace) {
			if (wrong_characters_count > 0) {
				wrong_characters_count -= 1;
				return;
			}

			if (character_index > 0) {
				character_index -= 1;
			}
			return;
		}
		// We overflowing the note's length, we don't add to any count
		if (character_index + wrong_characters_count >= notes_length) {
			return;
		}
		// Text must be perfect, if one error exists the rest is just wrong
		if (wrong_characters_count > 0) {
			wrong_characters_count += 1;
			return;
		}
		string character = "" + text_mesh.text[character_index];
		KeyCode expected = Event.KeyboardEvent(character).keyCode;
		if (received != expected) {
			wrong_characters_count += 1;
			Debug.Log("Unexpected key(" + received + ") expected: " + expected);
			return;
		}
		character_index += 1;
		if (character_index < notes_length) {
			return;
		}
		Debug.Log("Note fully taken!");
		taking_notes = false;
		prompt_panel.SetActive(false);
		player.enabled = true;
		notes_taken += 1;
	}

	void Update() {
		if (!taking_notes) return;
		update_text_mesh();
	}

	void update_text_mesh() {
		for (int i = 0; i < text_mesh.textInfo.characterCount; ++i) {
			// if the character is a space, the vertexIndex will be 0, which is the same as the first character. If you don't leave here, you'll keep modifying the first character's vertices, I believe this is a bug in the text mesh pro code
			if (!text_mesh.textInfo.characterInfo[i].isVisible) {
				continue;
			}
			Color32 myColor32;
			if (i < character_index) {
				myColor32 = new Color32(255, 255, 255, 255);
			} else if (wrong_characters_count > 0 && i < character_index + wrong_characters_count) {
				myColor32 = new Color32(255, 50, 50, 255);
			} else {
				myColor32 = new Color32(255, 255, 255, 255/2);
			}
			int meshIndex = text_mesh.textInfo.characterInfo[i].materialReferenceIndex;
			int vertexIndex = text_mesh.textInfo.characterInfo[i].vertexIndex;
			Color32[] vertexColors = text_mesh.textInfo.meshInfo[meshIndex].colors32;
			vertexColors[vertexIndex + 0] = myColor32;
			vertexColors[vertexIndex + 1] = myColor32;
			vertexColors[vertexIndex + 2] = myColor32;
			vertexColors[vertexIndex + 3] = myColor32;
		}
		text_mesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
	}
}
