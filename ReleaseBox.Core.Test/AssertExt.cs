using Common.Expect;

namespace ReleaseBox.Core.Test;

public static class AssertExt
{
    public static TOk ResultOk<TOk, TError>(Result<TOk, TError> result)
    {
        var errorDump = result.IsError(out var error) ? error.ToString() : "";
        Assert.True(result.IsOk(out var ok), $"Result expected to be 'ok', but was 'error' of value: {errorDump}");
        return ok;
    }

    public static TError ResultError<TOk, TError>(Result<TOk, TError> result)
    {
        var okDump = result.IsOk(out var ok) ? ok.ToString() : "";
        Assert.True(result.IsError(out var error), $"Result expected to be 'error', but was 'ok' of value: {okDump}");
        return error;
    }
}