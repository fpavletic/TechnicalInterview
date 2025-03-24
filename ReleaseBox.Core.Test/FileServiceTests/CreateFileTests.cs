using ReleaseBox.Core.Data.ErrorCodes;
using Xunit.Abstractions;

namespace ReleaseBox.Core.Test.FileServiceTests;

[Collection(nameof(FileServiceTests))]
public class CreateFileTests
{
    private readonly FileServiceTestsFixture _fixture;
    private readonly ITestOutputHelper _output;

    public CreateFileTests(FileServiceTestsFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CreateFileFailsWhenParentDirectoryDoesNotExist()
    {
        var error = AssertExt.ResultError(await _fixture.FileService.CreateFileAsync(-1, "file"));
        _output.WriteLine(error.ToString());
        Assert.Equal(CreateErrorCodes.ParentNotFound, error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateFileFailsWhenProvidedAnInvalidFileName(string fileName)
    {
        var error = AssertExt.ResultError(await _fixture.FileService.CreateFileAsync(0, fileName));
        _output.WriteLine(error.ToString());
        Assert.Equal(CreateErrorCodes.Invalid, error.Code);
    }

    [Fact]
    public async Task CreateFileFailsWhenAttemtpingToCreateADuplicate()
    {
        var fileName = Guid.NewGuid().ToString("N");
        AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(0, fileName));
        var error = AssertExt.ResultError(await _fixture.FileService.CreateFileAsync(0, fileName));
        _output.WriteLine(error.ToString());
        Assert.Equal(CreateErrorCodes.Duplicate, error.Code);
    }

    [Fact]
    public async Task CreateFileWorks()
    {        
        var fileName = Guid.NewGuid().ToString("N");
        var fileModel = AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(0, fileName));

        var discoveredFses = AssertExt.ResultOk(await _fixture.FileSystemEntryRepository.GetFileSystemEntries(0));
        Assert.Contains(discoveredFses, fse => !fse.IsDirectory && fse.FileSystemEntryId == fileModel.FileId && string.Equals(fse.FileSystemEntryName, fileName));
    }
}