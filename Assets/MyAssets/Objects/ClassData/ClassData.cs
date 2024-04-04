using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClassData : ScriptableObject {
	[SerializeField]
	private CategoryType category;
	[SerializeField]
	private BabelPage[] text_prompts_pages;
	[SerializeField]
	private CategoryQuestion[] question_prompts;
	[SerializeField]
	private int level_points;
	[SerializeField]
	private int points_till_questions_appear;

	public string[] text_prompts {
		get {
			return new List<BabelPage>(text_prompts_pages)
				.ConvertAll<string>(page => page.current_translation())
				.ToArray();
		}
	}

	public (int, ClassDataLevelPoint) get_random_point(float chances_for_question) {
		bool is_question = Random.value >= chances_for_question;
		if (!is_question || question_prompts.Length == 0) {
			return get_random_text_prompt();
		}
		return get_random_question_prompt();
	}

	public (int, ClassDataLevelPoint) get_random_question_prompt() {
		int index = 0;
		if (question_prompts.Length > 1) {
			index = Random.Range(0, question_prompts.Length);
		}
		var question = question_prompt(index);
		var point = ClassDataLevelPoint.from_question(question);
		return (index, point);
	}

	public (int, ClassDataLevelPoint) get_random_text_prompt() {
		int index = 0;
		if (text_prompts_pages.Length > 1) {
			index = Random.Range(0, text_prompts_pages.Length);
		}
		var page = page_prompt(index);
		var point = ClassDataLevelPoint.from_page(page);
		return (index, point);
	}

	public int get_level_points() => level_points;

	public int get_points_till_questions_appear() => points_till_questions_appear;

	public int get_text_prompts_count() => text_prompts_pages.Length;

	public string get_text_prompt(int index) => text_prompts_pages[index].current_translation();
	public BabelPage page_prompt(int index) => text_prompts_pages[index];

	public int get_questions_count() => question_prompts.Length;

	public CategoryQuestion question_prompt(int index) => question_prompts[index];
}

public struct ClassDataLevelPoint {
	public delegate R MatchPageLevelPoint<R>(BabelPage page);
	public delegate R MatchQuestionLevelPoint<R>(CategoryQuestion question);
	public static ClassDataLevelPoint from_page(BabelPage page) {
		var level_point = new ClassDataLevelPoint();
		level_point.text_page = page;
		level_point.question = null;
		return level_point;
	}
	public static ClassDataLevelPoint from_question(CategoryQuestion question) {
		var level_point = new ClassDataLevelPoint();
		level_point.text_page = null;
		level_point.question = question;
		return level_point;
	}

	private BabelPage text_page;
	private CategoryQuestion question;
	
	public R match<R>(MatchPageLevelPoint<R> match_page, MatchQuestionLevelPoint<R> match_question) {
		if (text_page != null) {
			return match_page(text_page);
		}
		if (question != null) {
			return match_question(question);
		}
		throw new System.InvalidOperationException("Unreachable");
	}
	public void match(System.Action<BabelPage> match_page, System.Action<CategoryQuestion> match_question) {
		if (text_page != null) {
			match_page(text_page);
			return;
		}
		if (question != null) {
			match_question(question);
			return;
		}
		throw new System.InvalidOperationException("Unreachable");
	}
}

public enum CategoryType {
	Philosophy,
	Literature,
	Psychology,
	Economics,
	Mathematics,
	Biology,
	Geography,
	Physics,
	Chemistry,
	Custom,
}

[System.Serializable]
public class CategoryQuestion {
	[SerializeField]
	private BabelPage question_page;
	[SerializeField]
	private BabelPage correct_answer_page;
	[SerializeField]
	private bool is_space_sensitive = false;
	[SerializeField]
	private bool is_case_sensitive = false;

	public BabelPage get_question_page() => question_page;
	public BabelPage get_correct_answer_page() => correct_answer_page;

	public string get_question() {
		return question_page.current_translation();
	}
	public string get_correct_answer() {
		return correct_answer_page.current_translation();
	}

	public bool is_correct_answer(string given_answer) {
		var expected_answer = get_correct_answer();
		var comparison_type = System.StringComparison.Ordinal;
		given_answer = given_answer.Trim();
		expected_answer = expected_answer.Trim();
		if (is_case_sensitive) {
			comparison_type = System.StringComparison.OrdinalIgnoreCase;
		}
		if (!is_space_sensitive) {
			given_answer = given_answer.Replace(" ", string.Empty);
			expected_answer = expected_answer.Replace(" ", string.Empty);
		}
		return given_answer.Equals(expected_answer, comparison_type);
	}

}
