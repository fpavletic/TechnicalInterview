using System.Diagnostics.CodeAnalysis;

namespace Common.Expect;

public abstract class Option<TValue> : IEquatable<Option<TValue>>
{
    // large prime
    private const int NoneHashCodeValue = 1043401;


    public bool Equals(Option<TValue>? other)
    {
        return other is not null && this == other;
    }


    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;

        if (ReferenceEquals(this, obj)) return true;

        if (obj.GetType() != GetType()) return false;

        return Equals((Option<TValue>)obj);
    }


    public override int GetHashCode()
    {
        return MapOrElse(value => EqualityComparer<TValue>.Default.GetHashCode(value!), () => NoneHashCodeValue);
    }


    public static implicit operator Option<TValue>(TValue value)
    {
        return new Some<TValue>(value);
    }


    public static implicit operator Option<TValue>(None _)
    {
        return None<TValue>.CachedInstance;
    }


    public abstract bool IsSome();


    public abstract bool IsSome([NotNullWhen(true)] out TValue? value);


    public bool IsNone()
    {
        return !IsSome();
    }


    public abstract TValue Expect();


    public abstract void ExpectNone();


    public abstract Option<TMapped> Map<TMapped>(Func<TValue, TMapped> mapper);


    public abstract TMapped MapOr<TMapped>(TMapped @default, Func<TValue, TMapped> mapper);


    public abstract TMapped MapOrElse<TMapped>(Func<TValue, TMapped> mapper, Func<TMapped> noneMapper);


    public abstract Option<TMapped> FlatMap<TMapped>(Func<TValue, Option<TMapped>> mapper);


    public abstract Option<TMapped> FlatMapOrElse<TMapped>(Func<TValue, Option<TMapped>> someMapper,
        Func<Option<TMapped>> noneMapper);


    public abstract Result<TValue, TError> Result<TError>(TError error);


    public abstract Result<TValue, TError> Result<TError>(Func<TError> errorFunc);


    public abstract TValue UnwrapOr(TValue @default);


    public abstract TValue UnwrapOr(Func<TValue> defaultProducer);


    public abstract void Match(Action<TValue> someMatch, Action noneMatch);


    public abstract Option<TValue> OnSome(Action<TValue> someMatch);


    public abstract Option<TValue> OnNone(Action noneMatch);


    public static bool operator ==(Option<TValue> lhs, Option<TValue> rhs)
    {
        // both null so they equal
        if (ReferenceEquals(lhs, null) && ReferenceEquals(null, rhs)) return true;

        // one null but not other, obviously not equal
        if (ReferenceEquals(lhs, null) ^ ReferenceEquals(null, rhs)) return false;

        // none of them null, so check if they are same ref
        if (ReferenceEquals(lhs, rhs)) return true;

        // ok, they are not same ref and not null so check if they are both none
        if (lhs!.IsNone() && rhs!.IsNone()) return true;

        // at least one of them is some, so check if both are
        if (lhs.IsSome(out var lhsValue) && rhs!.IsSome(out var rhsValue))
        {
            return EqualityComparer<TValue>.Default.Equals(lhsValue, rhsValue);
        }

        // one is none while other is some so they differ
        return false;
    }


    public static bool operator !=(Option<TValue> lhs, Option<TValue> rhs)
    {
        return !(lhs == rhs);
    }


    public string? ToString(Func<TValue, string> formatter)
    {
        return IsSome(out var value) ? formatter(value) : ToString();
    }
}

public sealed class Some<T> : Option<T>
{
    public Some([NotNull]T value)
    {
        Value = value;
    }
    
    [NotNull]
    public T Value { get; }


    public override bool IsSome()
    {
        return true;
    }


    public override bool IsSome([NotNullWhen(true)] out T? value)
    {
        value = Value;
        return true;
    }


    public override T Expect()
    {
        return Value;
    }


    public override void ExpectNone()
    {
        throw new OptionException($"Option was {nameof(Some<T>)} when {nameof(None)} was expected");
    }


    public override Option<TMapped> Map<TMapped>(Func<T, TMapped> mapper)
    {
        return mapper(Value);
    }


    public override TMapped MapOr<TMapped>(TMapped @default, Func<T, TMapped> mapper)
    {
        return mapper(Value);
    }


    public override TMapped MapOrElse<TMapped>(Func<T, TMapped> mapper, Func<TMapped> noneMapper)
    {
        return mapper(Value);
    }


    public override Option<TMapped> FlatMap<TMapped>(Func<T, Option<TMapped>> mapper)
    {
        return mapper(Value);
    }


    public override Option<TMapped> FlatMapOrElse<TMapped>(Func<T, Option<TMapped>> someMapper,
        Func<Option<TMapped>> noneMapper)
    {
        return someMapper(Value);
    }


    public override Result<T, TError> Result<TError>(TError error)
    {
        return Value;
    }


    public override Result<T, TError> Result<TError>(Func<TError> errorFunc)
    {
        return Value;
    }

    public override T UnwrapOr(T @default)
    {
        return Value;
    }

    public override T UnwrapOr(Func<T> defaultProducer)
    {
        return Value;
    }

    public override void Match(Action<T> someMatch, Action noneMatch)
    {
        someMatch(Value);
    }

    public override Option<T> OnSome(Action<T> someMatch)
    {
        Match(someMatch, () => { });
        return this;
    }

    public override Option<T> OnNone(Action noneMatch)
    {
        return this;
    }

    public override string? ToString()
    {
        return Value.ToString();
    }
}

public sealed class None<T> : Option<T>
{
    internal static readonly None<T> CachedInstance = new();

    public override Option<T> OnSome(Action<T> someMatch)
    {
        return this;
    }

    public override Option<T> OnNone(Action noneMatch)
    {
        Match(_ => { }, noneMatch);
        return this;
    }

    public override bool IsSome()
    {
        return false;
    }

    public override bool IsSome([NotNullWhen(true)]out T? value)
    {
        value = default;
        return false;
    }

    public override T Expect()
    {
        throw new OptionException($"Option was {nameof(None)} when {nameof(Some<T>)} was expected");
    }

    public override void ExpectNone()
    {
        // intentionally empty
    }

    public override Option<TMapped> Map<TMapped>(Func<T, TMapped> mapper)
    {
        return None.Value;
    }

    public override TMapped MapOr<TMapped>(TMapped @default, Func<T, TMapped> mapper)
    {
        return @default;
    }

    public override TMapped MapOrElse<TMapped>(Func<T, TMapped> mapper, Func<TMapped> noneMapper)
    {
        return noneMapper();
    }

    public override Option<TMapped> FlatMap<TMapped>(Func<T, Option<TMapped>> mapper)
    {
        return None.Value;
    }

    public override Option<TMapped> FlatMapOrElse<TMapped>(Func<T, Option<TMapped>> someMapper,
        Func<Option<TMapped>> noneMapper)
    {
        return noneMapper();
    }

    public override Result<T, TError> Result<TError>(TError error)
    {
        return error;
    }

    public override Result<T, TError> Result<TError>(Func<TError> errorFunc)
    {
        return errorFunc();
    }

    public override T UnwrapOr(T @default)
    {
        return @default;
    }

    public override T UnwrapOr(Func<T> defaultProducer)
    {
        return defaultProducer();
    }

    public override void Match(Action<T> someMatch, Action noneMatch)
    {
        noneMatch();
    }

    public override string ToString()
    {
        return $"None<{typeof(T).Name}>";
    }
}

public readonly struct None
{
    public static readonly None Value = new();
}

public class OptionException : Exception
{
    public OptionException(string message) : base(message)
    {
    }

    public OptionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}