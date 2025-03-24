namespace Common.Expect;

public static class TaskExtensions
{
    public static Task<T> AsCompletedTask<T>(this T result)
    {
        return Task.FromResult(result);
    }
    

    public static async Task<Result<TMapped, TError>> Then<TOk, TError, TMapped>(
        this Task<Result<TOk, TError>> asyncResult, Func<TOk, Result<TMapped, TError>> mapper)
    {
        var result = await asyncResult.ConfigureAwait(false);
        return result.Then(mapper);
    }

    
    public static async Task<Result<TMapped, TError>> ThenAsync<TOk, TError, TMapped>(
        this Task<Result<TOk, TError>> asyncResult, Func<TOk, Task<Result<TMapped, TError>>> mapper)
    {
        var result = await asyncResult.ConfigureAwait(false);
        return await result.ThenAsync(mapper).ConfigureAwait(false);
    }


    public static async Task<Result<TOk, TError>> OnSuccess<TOk, TError>(
        this Task<Result<TOk, TError>> asyncResult, Action<TOk> okAction)
    {
        var result = await asyncResult.ConfigureAwait(false);
        result.OnSuccess(okAction);
        return result;
    }


    public static async Task<Result<TAlt, TError>> Map<TOk, TAlt, TError>(this Task<Result<TOk, TError>> asyncResult,
        Func<TOk, TAlt> mapper)
    {
        var result = await asyncResult.ConfigureAwait(false);
        return result.Map(mapper);
    }


    public static async Task<Result<TOk, TAlt>> MapError<TOk, TError, TAlt>(this Task<Result<TOk, TError>> asyncResult,
        Func<TError, TAlt> mapper)
    {
        var result = await asyncResult.ConfigureAwait(false);
        return result.MapError(mapper);
    }


    public static async Task<Result<TOkAlt, TErrorAlt>> Transform<TOk, TError, TOkAlt, TErrorAlt>(
        this Task<Result<TOk, TError>> asyncResult,
        Func<TOk, TOkAlt> okMapper, Func<TError, TErrorAlt> errorMapper)
    {
        var result = await asyncResult.ConfigureAwait(false);
        return result.Transform(okMapper, errorMapper);
    }

    public static async Task<TMapped> Match<TOk, TError, TMapped>(this Task<Result<TOk, TError>> asyncResult,
        Func<TOk, TMapped> okMapper, Func<TError, TMapped> errorMapper)
    {
        var result = await asyncResult.ConfigureAwait(false);
        return result.Match(okMapper, errorMapper);
    }
}