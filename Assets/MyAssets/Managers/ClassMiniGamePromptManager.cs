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
	private ClassData _class_data;
	public ClassData data {
		set {
			_class_data = value;
			if (value == null) { return; }
			copy_prompts_tracker = new CategoryPointsTracker(
				_class_data.get_random_text_prompt,
				_class_data.get_text_prompts_count
			);
			question_prompts_tracker = new CategoryPointsTracker(
				_class_data.get_random_question_prompt,
				_class_data.get_questions_count
			);
		}
		get => _class_data;
	}
	private CategoryPointsTracker copy_prompts_tracker;
	private CategoryPointsTracker question_prompts_tracker;

	private List<ClassDataLevelPoint> class_points = new List<ClassDataLevelPoint>();

	private bool capturing_key_inputs;
	private UserTypingToTextTracker user_typing;

	private int class_point_index = 0;
	private int active_class_point_index = 0;
	public int completed_class_points { get; private set; }

	public bool is_category_done {
		get {
			if (data == null) {
				return false;
			}
			if (class_point_index < data.get_level_points()) {
				return false;
			}
			return true;
		}
	}

	public delegate void EventFinishedPoint(int point_index, bool finished_correctly);
	public event EventFinishedPoint finished_point;

	public void increase_class_point() {
		if (is_category_done) {
			return;
		}
		class_point_index += 1;
		bool can_request_question = class_point_index >= data.get_points_till_questions_appear();
		ClassDataLevelPoint point;
		if (!can_request_question) {
			point = get_random_point_try_non_repeating(copy_prompts_tracker);
			load_text_prompt();
			class_points.Add(point);
			return;
		}
		int index;
		(index, point) = data.get_random_point(0.6f);
		point = point.match(
				_ => {
					if (!copy_prompts_tracker.used_indexes.Contains(index)) {
						return point;
					}
					return get_random_point_try_non_repeating(copy_prompts_tracker);
				},
				_ => {
					if (!question_prompts_tracker.used_indexes.Contains(index)) {
						return point;
					}
					return get_random_point_try_non_repeating(question_prompts_tracker);
				}
		);
		load_point_prompt(point);
		class_points.Add(point);
	}

	private ClassDataLevelPoint get_random_point_try_non_repeating(CategoryPointsTracker tracker) {
		const int MAX_TRIES = 1000;
		int tries = 0;
		int index;
		int points_count = tracker.points_count;
		if (points_count < 1) {
			throw new System.IndexOutOfRangeException("Category points is empty and tried to access index 0");
		}
		if (points_count == 1) {
			return tracker.rand_point().Item2;
		}
		ClassDataLevelPoint point;
		var used_indexes = tracker.used_indexes;
		if (used_indexes.Count >= points_count) {
			used_indexes.Clear();
		}
		(index, point) = tracker.rand_point();
		while(used_indexes.Contains(index) && tries < MAX_TRIES) {
			(index, point) = tracker.rand_point();
			tries += 1;
		}
		tracker.add_index(index);
		return point;
	}

	public void load_text_prompt() {
		var point = get_random_point_try_non_repeating(copy_prompts_tracker);
		load_point_prompt(point);
	}
	public void load_question_prompt() {
		var point = get_random_point_try_non_repeating(question_prompts_tracker);
		load_point_prompt(point);
	}

	private void load_point_prompt(ClassDataLevelPoint point) {
		point.match(
			page => {
				text_prompt_panel.text_page = page;
				string translation = page.current_translation();
				user_typing = UserTypingToTextTracker.Create(translation);
			},
			question => {
				question_prompt_panel.text_page = question.get_question_page();
			}
		);
	}

	public void hide_panels() {
		text_prompt_panel.hide();
		question_prompt_panel.hide();
	}

	public void show_active_point_panel() {
		hide_panels();
		if (class_points.Count == 0) {
			return;
		}
		active_class_point_index = class_point_index;
		var active_point = class_points[class_points.Count - 1];
		active_point.match(
			_ => {
				text_prompt_panel.show();
				capturing_key_inputs = true;
			},
			_ => {
				question_prompt_panel.show();
				capturing_key_inputs = false;
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

		completed_class_points += 1;
		finished_point?.Invoke(active_class_point_index, true);
		//flag_active_note_copied();
		//close_notebook();
	}
}

class CategoryPointsTracker {
	public delegate (int, ClassDataLevelPoint) GetNewDataLevelPoint();
	public delegate int GetPointsCount();
	public List<int> used_indexes { private set; get; } = new List<int>() ;
	GetNewDataLevelPoint points_getter;
	GetPointsCount points_count_getter;

	public CategoryPointsTracker(GetNewDataLevelPoint category_points_getter, GetPointsCount category_points_count_getter) {
		points_getter = category_points_getter;
		points_count_getter = category_points_count_getter;
	}

	public (int, ClassDataLevelPoint) rand_point() {
		return points_getter();
	}
	public int points_count {
		get => points_count_getter();
	}

	public void add_index(int index) {
		if (used_indexes.Contains(index)) {
			return;
		}
		used_indexes.Add(index);
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
