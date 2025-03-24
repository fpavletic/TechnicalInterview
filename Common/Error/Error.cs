using Common.Expect;

namespace Common.Error;

public sealed record Error<TEnum> where TEnum : struct, Enum
{
    public Error(TEnum code, string description) : this(code, description, None.Value)
    {
    }


    public Error(TEnum code, string description, Option<Exception> exception)
    {
        Exception = exception;
        Description = description;
        Code = code;
    }


    public TEnum Code { get; }


    public string Description { get; }


    public Option<Exception> Exception { get; }


    public Error<TOtherEnum> ConvertTo<TOtherEnum>(Func<TEnum, TOtherEnum> mapper) where TOtherEnum : struct, Enum
    {
        return new Error<TOtherEnum>(mapper(Code), Description, Exception);
    }


    public override string ToString()
    {
        return $"Code={Code} Description={Description} Exception={Exception}";
    }
}