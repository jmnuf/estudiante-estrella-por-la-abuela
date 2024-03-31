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

	public string[] text_prompts {
		get {
			return new List<BabelPage>(text_prompts_pages)
				.ConvertAll<string>(page => page.current_translation())
				.ToArray();
		}
	}

	public int get_text_prompts_count() {
		return text_prompts_pages.Length;
	}

	public string get_text_prompt(int index) {
		return text_prompts_pages[index].current_translation();
	}

	public int get_questions_count() {
		return question_prompts.Length;
	}

	public CategoryQuestion get_question_prompt(int index) {
		return question_prompts[index];
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
