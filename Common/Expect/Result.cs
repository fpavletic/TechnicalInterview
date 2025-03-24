using System.Diagnostics.CodeAnalysis;

namespace Common.Expect;

public sealed class Result<TOk, TError>
{
    private readonly TError? _err;
    private readonly bool _isOk;
    private readonly TOk? _ok;

    private Result(TOk ok)
    {
        _ok = ok;
        _isOk = true;
    }

    private Result(TError error)
    {
        _err = error;
        _isOk = false;
    }


    public static Result<TOk, TError> Ok(TOk ok)
    {
        return new Result<TOk, TError>(ok);
    }


    public static Result<TOk, TError> Error(TError error)
    {
        return new Result<TOk, TError>(error);
    }


    public static implicit operator Result<TOk, TError>(TOk ok)
    {
        return new Result<TOk, TError>(ok);
    }


    public static implicit operator Result<TOk, TError>(TError err)
    {
        return new Result<TOk, TError>(err);
    }


    public bool IsOk()
    {
        return _isOk;
    }


    public bool IsOk([NotNullWhen(true)] out TOk? value)
    {
        if (IsOk())
        {
            value = _ok!;
            return true;
        }

        value = default;
        return false;
    }


    public bool IsError()
    {
        return !IsOk();
    }


    public bool IsError([NotNullWhen(true)] out TError? value)
    {
        if (IsError())
        {
            value = _err!;
            return true;
        }

        value = default;
        return false;
    }


    public TOk Expect()
    {
        return IsOk(out var ok)
            ? ok
            : throw new ResultException<TError>(ExpectError(), $"Expected {nameof(Ok)} but it was {nameof(Error)}");
    }


    public TError ExpectError()
    {
        return IsError(out var err)
            ? err
            : throw new ResultException<TOk>(Expect(), $"Expected {nameof(Error)} but result was {nameof(Ok)}");
    }


    public Result<TAlt, TError> Map<TAlt>(Func<TOk, TAlt> mapper)
    {
        return IsOk(out var ok) ? mapper(ok) : new Result<TAlt, TError>(ExpectError());
    }


    public Result<TOk, TAlt> MapError<TAlt>(Func<TError, TAlt> mapper)
    {
        return IsError(out var err) ? new Result<TOk, TAlt>(mapper(err)) : new Result<TOk, TAlt>(Expect());
    }


    public TResult MapOr<TResult>(TResult @default, Func<TOk, TResult> okMapper)
    {
        return IsOk(out var ok) ? okMapper(ok) : @default;
    }


    public TMapped MapOrElse<TMapped>(Func<TOk, TMapped> okMapper, Func<TError, TMapped> errorMapper)
    {
        return IsOk(out var ok) ? okMapper(ok) : errorMapper(ExpectError());
    }


    public Result<TMapped, TError> Then<TMapped>(Func<TOk, Result<TMapped, TError>> mapper)
    {
        return IsOk(out var ok) ? mapper(ok) : ExpectError();
    }


    public async Task<Result<TMapped, TError>> ThenAsync<TMapped>(Func<TOk, Task<Result<TMapped, TError>>> mapper)
    {
        return IsOk(out var ok) ? await mapper(ok).ConfigureAwait(false) : ExpectError();
    }


    public Result<TAlt, TError> And<TAlt>(Result<TAlt, TError> other)
    {
        return Then(_ => other);
    }


    public Result<TOk, TError> Or(Result<TOk, TError> other)
    {
        return MapOr(other, ok => ok);
    }


    public Result<TOk, TError> OrElse(Func<Result<TOk, TError>> otherProducer)
    {
        return IsOk() ? this : otherProducer();
    }


    public TOk UnwrapOr(Func<TError, TOk> errorMapper)
    {
        return IsOk(out var ok) ? ok : errorMapper(ExpectError());
    }


    public TOk UnwrapOr(TOk @default)
    {
        return IsOk(out var ok) ? ok : @default;
    }


    public Result<TOk, TError> OnError(Action<TError> errorAction)
    {
        if (IsError(out var err)) errorAction(err);

        return this;
    }


    public Result<TOk, TError> OnSuccess(Action<TOk> okAction)
    {
        if (IsOk(out var ok)) okAction(ok);

        return this;
    }


    public TMapped Match<TMapped>(Func<TOk, TMapped> okMapper, Func<TError, TMapped> errorMapper) =>
        IsOk(out var ok) 
            ? okMapper(ok)
            : errorMapper(ExpectError());


    public Result<TOkAlt, TErrorAlt> Transform<TOkAlt, TErrorAlt>(Func<TOk, TOkAlt> okMapper,
        Func<TError, TErrorAlt> errorMapper)
    {
        return IsOk(out var ok)
            ? Result<TOkAlt, TErrorAlt>.Ok(okMapper(ok))
            : Result<TOkAlt, TErrorAlt>.Error(errorMapper(ExpectError()));
    }


    public Option<TOk> Option()
    {
        return IsOk(out var ok) ? new Some<TOk>(ok) : None.Value;
    }


    public Task<Result<TOk, TError>> AsTask()
    {
        return Task.FromResult(this);
    }
}

public class ResultException<TError> : Exception
{
    public ResultException(TError unhandledError) : this(unhandledError, string.Empty)
    {
    }


    public ResultException(TError unhandledError, string message) : this(unhandledError, message, null)
    {
    }


    public ResultException(TError unhandledError, string message, Exception? innerException) : base(message,
        innerException)
    {
        UnhandledError = unhandledError;
    }


    public TError UnhandledError { get; }
}