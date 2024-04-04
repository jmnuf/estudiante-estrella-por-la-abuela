using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassMiniGamePromptManager : MonoBehaviour {
	[SerializeField]
	private UITextPanel text_prompt_panel;
	[SerializeField]
	private UITextPanel question_prompt_panel;
	[SerializeField]
	private UITextPanel class_done_panel;
	public ClassData data;
	private List<int> text_prompts_indexes = new List<int>();
	private List<int> questions_prompts_indexes = new List<int>();

	private bool capturing_key_inputs;
	private UserTypingToTextTracker user_typing;

	private int class_point = 0;

	public void increase_class_point() {
		class_point += 1;
		bool can_request_question = class_point >= data.get_points_till_questions_appear();
		if (!can_request_question) {
			load_text_prompt();
			return;
		}
		var (index, point) = data.get_random_point(0.6f);
	}

	public void load_text_prompt() {
		int index;
		ClassDataLevelPoint point;
		(index, point) = data.get_random_text_prompt();
		while (text_prompts_indexes.Contains(index)) {
			(index, point) = data.get_random_text_prompt();
		}
		text_prompts_indexes.Add(index);
		point.match(
			page => {
				string translation = page.current_translation();
				text_prompt_panel.text_page = page;
				user_typing = UserTypingToTextTracker.Create(translation);
			},
			_question => {
				throw new System.InvalidProgramException("Unreachable");
			}
		);
	}

	private void OnGUI() {
		if (!capturing_key_inputs) return;

		Event ev = Event.current;
		if (!user_typing.is_event_valid(ev)) {
			return;
		}

		if (!user_typing.use_event_against_prompt(ev)) {
			return;
		}

		//flag_active_note_copied();
		//close_notebook();
	}
}

struct UserTypingToTextTracker {
	private InputKeyValidator validator;

	public string prompt_text { private set; get; }
	public int prompt_length { private set; get; }
	public int last_correct_character_index { private set; get; }
	public int wrong_characters_count { private set; get; }

	public static UserTypingToTextTracker Create(string prompt) {
		UserTypingToTextTracker user_type = new UserTypingToTextTracker();
		user_type.validator = new InputKeyValidator();
		user_type.prompt_text = prompt;
		user_type.prompt_length = prompt.Length;
		user_type.last_correct_character_index = 0;
		user_type.wrong_characters_count = 0;
		return user_type;
	}

	public float user_progress() {
		float index = (float)last_correct_character_index;
		float total = (float)prompt_length;
		float perct = index / total;
		return perct;
	}

	public bool is_event_valid(Event ev) {
		if (validator.ignore_input_event(ev)) {
			return false;
		}

		KeyCode key_code = ev.keyCode;
		char character = ev.character;
		if (!validator.is_allowed_special_char(character) && !validator.is_keycode_valid(key_code)) {
			return false;
		}

		return true;
	}

	public bool use_event_against_prompt(Event ev) {
		if (last_correct_character_index >= prompt_length) {
			return true;
		}
		if (wrong_characters_count < 0) {
			wrong_characters_count = 0;
		}
		KeyCode key_code = ev.keyCode;
		if (key_code == KeyCode.Backspace) {
			if (wrong_characters_count > 0) {
				wrong_characters_count -= 1;
				return false;
			}

			if (last_correct_character_index > 0) {
				last_correct_character_index -= 1;
			}
			return false;
		}
		// We overflowing the note's length, we don't add to any count
		if (last_correct_character_index + wrong_characters_count >= prompt_length) {
			return false;
		}
		// Text must be perfect, if one error exists the rest is just wrong
		if (wrong_characters_count > 0) {
			wrong_characters_count += 1;
			return false;
		}
		char user_character = ev.character;
		char expected_character = prompt_text[last_correct_character_index + 1];
		if (user_character != expected_character) {
			wrong_characters_count += 1;
			// Debug.Log("Unexpected key(" + user_character + ") expected: " + expected_character);
			return false;
		}
		last_correct_character_index += 1;
		bool is_finished = last_correct_character_index >= prompt_length;
		return is_finished;
	}
}

struct InputKeyValidator {
	static readonly char[] characters = new char[] {
		'à', 'á', 'â', 'ã', 'ä', 'å', 'ă',
		'æ', 'ç',
		'è', 'é', 'ê', 'ë', 'ĕ',
		'ì', 'í', 'î', 'ï',
		'ñ',
		'ò', 'ó', 'ô', 'õ', 'ö',
		'ù', 'ú', 'û', 'ü'
	};

	bool is_keycode_a_letter(KeyCode kc) {
		return (KeyCode.A <= kc && kc <= KeyCode.Z);
	}

	bool is_keycode_a_digit(KeyCode kc) {
		return (KeyCode.Keypad0 <= kc && kc <= KeyCode.Keypad9) || (KeyCode.Alpha0 <= kc && kc <= KeyCode.Alpha9);
	}

	bool is_keycode_a_quote(KeyCode kc) {
		return kc == KeyCode.DoubleQuote || kc == KeyCode.Quote;
	}

	public bool is_keycode_valid(KeyCode kc) {
		if (is_keycode_a_letter(kc)) return true;
		if (is_keycode_a_digit(kc)) return true;
		if (is_keycode_a_quote(kc)) return true;
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

	public bool is_allowed_special_char(char event_char) {
		return System.Array.Exists(characters, c => c == event_char);
	}

	public bool ignore_input_event(UnityEngine.Event e) {
		if (!e.isKey) { return true; }
		// If it's a NULL termination, we ignore it as we kinda require an actual character
		if (e.character == '\0') { return true; }
		if (e.type != EventType.KeyDown) { return true; }
		return false;
	}
}
