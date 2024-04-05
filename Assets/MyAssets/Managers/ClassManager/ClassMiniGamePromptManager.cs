using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClassManager))]
public class ClassMiniGamePromptManager : MonoBehaviour {
	[SerializeField]
	private UITextPanel text_prompt_panel;
	[SerializeField]
	private UITextInputPanel question_prompt_panel;
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
	public ClassManager manager { get; private set; }
	public event System.Action<ClassDataLevelPoint> on_new_point_added;
	public event System.Action on_copy_text_point_loaded;
	public event System.Action on_question_point_loaded;
	public event System.Action<ClassDataLevelPoint> on_some_point_loaded;
	private CategoryPointsTracker copy_prompts_tracker;
	private CategoryPointsTracker question_prompts_tracker;

	private List<ClassDataLevelPoint> class_points = new List<ClassDataLevelPoint>();
	public int class_points_count {
		get => class_points.Count;
	}

	public bool capturing_key_inputs { get; private set; }
	private UserTypingToTextTracker user_typing;
	public bool answering_question { get; private set; }
	public bool is_playing_minigame {
		get => capturing_key_inputs || answering_question;
	}

	public int class_point_index { get; private set; } = -1;
	public int active_class_point_index { get; private set; } = -1;
	public int completed_class_points { get; private set; }

	public bool is_category_done {
		get {
			if (data == null) {
				return true;
			}
			if (class_point_index >= data.get_level_points()) {
				return true;
			}
			return false;
		}
	}

	private void Awake() {
		manager = GetComponent<ClassManager>();
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
			class_points.Add(point);
			on_new_point_added?.Invoke(point);
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
		class_points.Add(point);
		on_new_point_added?.Invoke(point);
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
				on_copy_text_point_loaded?.Invoke();
			},
			question => {
				question_prompt_panel.text_page = question.get_question_page();
				on_question_point_loaded?.Invoke();
			}
		);
		on_some_point_loaded?.Invoke(point);
	}

	public void hide_game_panels() {
		Debug.Log("Hiding minigame panels");
		capturing_key_inputs = false;
		text_prompt_panel.hide();
		question_prompt_panel.hide();
	}

	public void hide_results_panel() {
		class_done_panel.hide();
	}

	public void show_active_point_panel() {
		hide_game_panels();
		if (class_points.Count == 0) {
			return;
		}
		active_class_point_index = class_point_index;
		var active_point = class_points[active_class_point_index];
		load_point_prompt(active_point);
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

	public IEnumerator show_class_results(float results) {
		int max_points = data.get_level_points();
		float passing_score = ((float) max_points) * 0.7f;
		Debug.Log($"Class results: {results} out of {max_points}");
		Color32 text_color = (results < passing_score) ? Color.red : Color.green;
		string text = $"{results.ToString("N2")} / {max_points.ToString("N")}";
		class_done_panel.tmp_text.text = text;
		class_done_panel.show();
		
		yield return new WaitForSeconds(0.5f);

		for (int i = 0; i <= text.Length; ++i) {
			class_done_panel.color_text_letters((idx, _) => {
				if (idx > i) {
					return (Color32) Color.white;
				}
				return text_color;
			});

			yield return new WaitForSeconds(0.15f);
		}
	}

	public float interrupt_current_prompt() {
		if (!capturing_key_inputs && !answering_question) {
			return 0f;
		}
		
		var active_point = class_points[active_class_point_index];
		return active_point.match(
			_ => {
				// finished_point?.Invoke(active_class_point_index, false);
				return user_typing.user_progress();
			},
			question => {
				string user_inputted_text = question_prompt_panel.pop_input_text();
				bool is_correct_answer = question.is_correct_answer(user_inputted_text);
				// finished_point?.Invoke(active_class_point_index, is_correct_answer);
				if (is_correct_answer) {
					return 1f; // If the user just missed the time to submit it, it's fine still counts
				}
				return 0f;
			}
		);
	}

	private void OnGUI() {
		if (!capturing_key_inputs) {
			return;
		}

		Event ev = Event.current;
		if (!user_typing.is_event_valid(ev)) {
			return;
		}

		if (!user_typing.use_event_against_prompt(ev)) {
			return;
		}

		completed_class_points += 1;
		finished_point?.Invoke(active_class_point_index, true);
	}

	private void Update() {
		if (!capturing_key_inputs) {
			return;
		}
		update_prompt_text_mesh();
	}

	private void update_prompt_text_mesh() {
		int wrong_characters_count = user_typing.wrong_characters_count;
		int last_correct_character_index = user_typing.last_correct_character_index;
		text_prompt_panel.color_text_letters((idx, _) => {
			Color32 char_color;
			if (idx <= last_correct_character_index) {
				char_color = Color.white;
			} else if (wrong_characters_count > 0 && idx <= last_correct_character_index + wrong_characters_count) {
				char_color = Color.red;
			} else {
				Color color = Color.gray;
				color.a = 0.5f;
				char_color = color;
			}
			return char_color;
		});
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
		user_type.last_correct_character_index = -1;
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

		/*
		KeyCode key_code = ev.keyCode;
		char character = ev.character;
		if (!validator.is_allowed_special_char(character) && !validator.is_keycode_valid(key_code)) {
			return false;
		}
		*/

		return true;
	}

	public bool use_event_against_prompt(Event ev) {
		int next_char_index = last_correct_character_index + 1;
		if (next_char_index >= prompt_length) {
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

			if (last_correct_character_index >= 0) {
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
		char expected_character = prompt_text[next_char_index];
		if (user_character != expected_character) {
			wrong_characters_count += 1;
			Debug.Log($"Incorrect key at index {next_char_index}: {user_character} != {expected_character}");
			return false;
		}
		last_correct_character_index += 1;
		bool is_finished = last_correct_character_index + 1 >= prompt_length;
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
		if (e.character == '\0') {
			if (e.keyCode != KeyCode.Backspace) {
				return true;
			}
		}
		if (e.type != EventType.KeyDown) { return true; }
		return false;
	}
}
