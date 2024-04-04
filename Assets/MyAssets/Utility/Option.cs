using System;

public struct Option<T> {
	private OptionType type;
	private T inner_value;
	private Option(OptionType t, T val) {
		type = t;
		inner_value = val;
	}
	public delegate ReturnType OptionSomeMatchFn<ReturnType>(T val);
	public delegate ReturnType OptionNoneMatchFn<ReturnType>();

	public void match(Action<T> on_some, Action on_none) {
		switch (type) {
			case OptionType.Some:
				on_some(inner_value);
				return;
			case OptionType.None:
				on_none();
				return;
			default:
				throw new InvalidOperationException();
		}
	}
	public R match<R>(OptionSomeMatchFn<R> on_some, OptionNoneMatchFn<R> on_none) => type switch {
			OptionType.Some => on_some(inner_value),
			OptionType.None => on_none(),
			_ => throw new InvalidOperationException(),
	};

	public T unwrap() {
		if (type == OptionType.None) {
			throw new InvalidOperationException();
		}
		return inner_value;
	}
	public bool is_some() => type == OptionType.Some;
	public bool is_none() => type == OptionType.None;
	public bool Equals(Option<T> other) {
		if (type != other.type) { return false; }
		if (type == OptionType.None) { return true; }
		if (inner_value == null) {
			if (other.inner_value == null) { return true; }
			return false;
		}
		return inner_value.Equals(other.inner_value);
	}

	public override string ToString() {
		var self = this;
		return match(
			v => "Some(" + self.inner_value.ToString() + ")",
			() => "None"
		);
	}

	public static Option<T> Some(T val) => new Option<T>(OptionType.Some, val);
	public static Option<T> None { get => new Option<T>(OptionType.None, default); }
}

enum OptionType {
	Some,
	None,
}
