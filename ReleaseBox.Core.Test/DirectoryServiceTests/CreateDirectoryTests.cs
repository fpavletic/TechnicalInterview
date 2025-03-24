using ReleaseBox.Core.Data.ErrorCodes;
using Xunit.Abstractions;

namespace ReleaseBox.Core.Test.DirectoryServiceTests;

[Collection(nameof(DirectoryServiceTests))]
public class CreateDirectoryTests
{
    private readonly DirectoryServiceTestsFixture _fixture;
    private readonly ITestOutputHelper _output;

    public CreateDirectoryTests(DirectoryServiceTestsFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CreateDirectoryFailsWhenParentDirectoryDoesNotExist()
    {
        var error = AssertExt.ResultError(await _fixture.DirectoryService.CreateDirectoryAsync(-1, "dir"));
        _output.WriteLine(error.ToString());
        Assert.Equal(CreateErrorCodes.ParentNotFound, error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateDirectoryFailsWhenProvidedAnInvalidFileName(string dirName)
    {
        var error = AssertExt.ResultError(await _fixture.DirectoryService.CreateDirectoryAsync(0, dirName));
        _output.WriteLine(error.ToString());
        Assert.Equal(CreateErrorCodes.Invalid, error.Code);
    }

    [Fact]
    public async Task CreateDirectoryFailsWhenAttemtpingToCreateADuplicate()
    {
        var dirName = Guid.NewGuid().ToString("N");
        AssertExt.ResultOk(await _fixture.DirectoryService.CreateDirectoryAsync(0, dirName));
        var error = AssertExt.ResultError(await _fixture.DirectoryService.CreateDirectoryAsync(0, dirName));
        _output.WriteLine(error.ToString());
        Assert.Equal(CreateErrorCodes.Duplicate, error.Code);
    }                                                                         

    [Fact]
    public async Task CreateDirectoryWorks()
    {
        var dirName = Guid.NewGuid().ToString("N");
        var dirModel = AssertExt.ResultOk(await _fixture.DirectoryService.CreateDirectoryAsync(0, dirName));

        var discoveredFses = AssertExt.ResultOk(await _fixture.FileSystemEntryRepository.GetFileSystemEntries(0));
        Assert.Contains(discoveredFses, fse => fse.IsDirectory && fse.FileSystemEntryId == dirModel.DirectoryId && string.Equals(fse.FileSystemEntryName, dirName));
    }
}