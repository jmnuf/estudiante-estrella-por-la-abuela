using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestClassManager : MonoBehaviour {
	[Header("Data References")]
	public TestTeacher teacher;
	public TestNotebook notebook;
	public PlayerObject player;

	[Header("UI References")]
	public GameObject prompt_panel;
	public TextMeshProUGUI prompt_text_mesh;

	public GameObject class_done_panel;
	public TextMeshProUGUI results_text_mesh;

	public GameObject questions_panel;
	public TextMeshProUGUI questions_text_mesh;
	public TMP_InputField questions_input_field;

	[Header("Mini-game Information")]
	public ClassMaterial class_material;
	public string[] possible_notes_to_copy;
	[SerializeField]
	private bool use_questions;
	public PossibleQuestionAndAnswer[] possible_questions;

	private bool is_point_a_question = false;
	private bool is_answering_question = false;
	private List<int> questions_answered = new List<int>();
	private PossibleQuestionAndAnswer active_question;
	private int active_question_index;

	private bool is_copying_notes = false;
	private List<int> notes_copied = new List<int>();
	private int active_note_character_index = 0;
	private int wrong_characters_count = 0;
	private string active_note_string;
	private int active_note_index;
	private int active_note_length;
	private int notes_taken = 0;

	void Start() {
		// Setup callbacks
		teacher.new_important_point_called += on_new_important_point;
		teacher.on_interaction += on_teacher_interaction;
		teacher.on_class_dismissed += on_class_dismissed;
		notebook.on_note_taking_requested += on_notebook_note_taking_requested;
		questions_input_field.onEndEdit.AddListener(on_questions_input_field_end_edit);

		// Deactivate UI Game UI Panels
		close_ui_panels();
		class_done_panel.SetActive(false);

		int possible_note_sources = possible_notes_to_copy.Length + (use_questions ? possible_questions.Length : 0);
		int teachers_points_count = teacher.get_max_points();
		Debug.Assert(possible_note_sources >= teachers_points_count,
				"Not enough note sources provided ("
				+ possible_note_sources
				+ ") for the amount of points the teacher will set ("
				+ teachers_points_count + ")"
		);
	}

	void close_ui_panels() {
		prompt_panel.SetActive(false);
		questions_panel.SetActive(false);
	}

	void on_new_important_point(int _point, int _max_points) {
		active_question_index = -1;
		active_note_index = -1;

		Debug.Log("New important point detected from the class manager");
		if (use_questions) {
			if (notes_copied.Count >= possible_notes_to_copy.Length) {
				is_point_a_question = true;
			} else if ((active_question_index != -1 && possible_questions.Length > questions_answered.Count + 1) || possible_questions.Length > questions_answered.Count) {
				is_point_a_question = Random.value >= 0.5f;
			}
		}
		notebook.can_take_notes = true;
	}

	void on_teacher_interaction() {
		Debug.Log("Teacher interaction detected from the class manager");
		notebook.can_take_notes = true;
	}

	void on_class_dismissed() {
		Debug.Log("Can't take more notes as class is dismissed");

		// Disable player movement
		player.enabled = false;

		notebook.can_take_notes = false;
		if (is_answering_question) questions_panel.SetActive(false);
		if (is_copying_notes) prompt_panel.SetActive(false);

		int max_points = teacher.get_max_points();
		float points_made = (float) notes_taken;
		if (is_copying_notes) {
			float subpoints = ((float)active_note_character_index) / ((float)active_note_length);
			points_made += subpoints;
		}
		is_copying_notes = false;

		string results_str = "Class Finished!\n";
		results_str += points_made + " / " + max_points + " notes completed";
		results_text_mesh.text = results_str;
		class_done_panel.SetActive(true);
	}

	PossibleQuestionAndAnswer get_a_question_to_answer() {
		(var question, int index) = get_a_random_non_repeated_item(possible_questions, questions_answered, active_question_index);
		active_question_index = index;
		return question;
	}

	string get_a_note_to_copy() {
		(string note, int index) = get_a_random_non_repeated_item(possible_notes_to_copy, notes_copied, active_note_index);
		active_note_index = index;
		return note;
	}

	private (T, int) get_a_random_non_repeated_item<T>(T[] main_array, List<int> copies_indeces, int active_item_index) {
		int index = Random.Range(0, main_array.Length);
		const int max_tries = 100;
		int tries = 1;
		while (copies_indeces.Contains(index) && index != active_item_index && tries <= max_tries) {
			index = Random.Range(0, main_array.Length);
			tries += 1;
		}
		T item = main_array[index];
		return (item, index);
	}

	IEnumerator delayed_set_taking_notes(bool new_value) {
		yield return 0;
		is_copying_notes = new_value;
	}

	void on_notebook_note_taking_requested() {
		player.enabled = false;

		if (is_point_a_question) {
			questions_panel.SetActive(true);
			active_question = get_a_question_to_answer();

			questions_text_mesh.text = active_question.question;
			is_answering_question = true;
			questions_input_field.text = "";
			questions_input_field.ActivateInputField();
			return;
		}
		prompt_panel.SetActive(true);

		active_note_string = get_a_note_to_copy();
		active_note_length = active_note_string.Length;

		prompt_text_mesh.text = active_note_string;
		active_note_character_index = 0;
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
		if (!is_copying_notes) return;

		Event ev = Event.current;
		if (ev.type != EventType.KeyDown) return;
		KeyCode received = ev.keyCode;
		if (!is_key_valid(received)) return;
		if (received == KeyCode.Backspace) {
			if (wrong_characters_count > 0) {
				wrong_characters_count -= 1;
				return;
			}

			if (active_note_character_index > 0) {
				active_note_character_index -= 1;
			}
			return;
		}
		// We overflowing the note's length, we don't add to any count
		if (active_note_character_index + wrong_characters_count >= active_note_length) {
			return;
		}
		// Text must be perfect, if one error exists the rest is just wrong
		if (wrong_characters_count > 0) {
			wrong_characters_count += 1;
			return;
		}
		string character = "" + prompt_text_mesh.text[active_note_character_index];
		KeyCode expected = Event.KeyboardEvent(character).keyCode;
		if (received != expected) {
			wrong_characters_count += 1;
			Debug.Log("Unexpected key(" + received + ") expected: " + expected);
			return;
		}
		active_note_character_index += 1;
		if (active_note_character_index < active_note_length) {
			return;
		}

		flag_active_note_copied();
		close_notebook();
	}

	void flag_active_note_copied() {
		notes_copied.Add(active_note_index);
		notes_taken += 1;
	}

	void flag_active_question_answered(bool was_correct) {
		questions_answered.Add(active_question_index);
		notes_taken += was_correct ? 1 : 0; // If correct add one, if incorrect add zero
	}

	void close_notebook() {
		Debug.Log("Note fully taken!");
		is_copying_notes = false;
		is_answering_question = false;
		close_ui_panels();
		player.enabled = true;
	}

	void Update() {
		if (!is_copying_notes) return;
		update_prompt_text_mesh();
	}

	// Code from: https://forum.unity.com/threads/fixed-change-color-of-individual-characters-in-textmeshpro-text-ui.1278845/#post-8884554
	void update_prompt_text_mesh() {
		for (int i = 0; i < prompt_text_mesh.textInfo.characterCount; ++i) {
			// if the character is a space, the vertexIndex will be 0, which is the same as the first character. If you don't leave here, you'll keep modifying the first character's vertices, I believe this is a bug in the text mesh pro code
			if (!prompt_text_mesh.textInfo.characterInfo[i].isVisible) {
				continue;
			}
			Color32 myColor32;
			if (i < active_note_character_index) {
				myColor32 = new Color32(255, 255, 255, 255);
			} else if (wrong_characters_count > 0 && i < active_note_character_index + wrong_characters_count) {
				myColor32 = new Color32(255, 50, 50, 255);
			} else {
				myColor32 = new Color32(255, 255, 255, 255/2);
			}
			int meshIndex = prompt_text_mesh.textInfo.characterInfo[i].materialReferenceIndex;
			int vertexIndex = prompt_text_mesh.textInfo.characterInfo[i].vertexIndex;
			Color32[] vertexColors = prompt_text_mesh.textInfo.meshInfo[meshIndex].colors32;
			vertexColors[vertexIndex + 0] = myColor32;
			vertexColors[vertexIndex + 1] = myColor32;
			vertexColors[vertexIndex + 2] = myColor32;
			vertexColors[vertexIndex + 3] = myColor32;
		}
		prompt_text_mesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
	}

	void on_questions_input_field_end_edit(string text) {
		bool answer_is_correct = active_question.is_user_answer_correct(text);
		// TODO: Actually display this on screen with an icon or some text
		if (!answer_is_correct) {
			string expected = active_question.answer.Trim().ToLower();
			Debug.Log("Answer is incorrect! Expected: `" + expected + "`");
		} else {
			Debug.Log("Answer is correct!");
		}
		// Debug.Log("Submitted Answer: `" + text);
		flag_active_question_answered(answer_is_correct);
		close_notebook();
	}

}

public enum ClassMaterial {
	Language,
	Math,
	Philosophy
}

[System.Serializable]
public class PossibleQuestionAndAnswer {
	public string question;
	public string answer;
	public bool ignores_white_space = false;

	public bool is_user_answer_correct(string user_answer) {
		string correct_answer = answer.Trim().ToLower();
		user_answer = user_answer.Trim().ToLower();
		if (correct_answer == user_answer) {
			return true;
		}

		if (ignores_white_space) {
			string no_white_space = "";
			foreach(char c in user_answer) {
				if (char.IsWhiteSpace(c)) continue;
				no_white_space += "" + c;
			}

			if (correct_answer == no_white_space) {
				return true;
			}
		}

		return false;
	}
}
