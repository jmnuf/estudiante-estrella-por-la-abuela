using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Result<TValue, TError> {
	private ResultType type;
	private TValue inner_value;
	private TError inner_error;
	public delegate TRet MatchOkResult<TRet>(TValue val);
	public delegate TRet MatchErrResult<TRet>(TError err);

	private Result(ResultType res_type, TValue val, TError err) {
		type = res_type;
		inner_value = val;
		inner_error = err;
	}

	public R match<R>(MatchOkResult<R> match_ok, MatchErrResult<R> match_err) => type switch {
		ResultType.Ok => match_ok(inner_value),
		ResultType.Err => match_err(inner_error),
		_ => throw new InvalidOperationException(),
	};

	public TValue unwrap() {
		if (type == ResultType.Err) {
			if (inner_error is Exception) {
				Exception exception = inner_error as Exception;
				throw exception;
			}
			throw new Exception(inner_error.ToString());
		}
		return inner_value;
	}

	public bool is_ok() => type == ResultType.Ok;
	public bool is_err() => type == ResultType.Err;

	public Option<TValue> to_option() => match<Option<TValue>>(
		(val) => Option<TValue>.Some(val),
		(_er) => Option<TValue>.None
	);

	public static Result<TValue, TError> Ok(TValue val) => new Result<TValue, TError>(ResultType.Ok, val, default);
	public static Result<TValue, TError> Err(TError err) => new Result<TValue, TError>(ResultType.Err, default, err);
}

enum ResultType {
	Ok,
	Err,
}
