using ReleaseBox.Core.Data.ErrorCodes;
using Xunit.Abstractions;

namespace ReleaseBox.Core.Test.DirectoryServiceTests;

[Collection(nameof(DirectoryServiceTests))]
public class DeleteDirectoryTests
{
    private readonly DirectoryServiceTestsFixture _fixture;
    private readonly ITestOutputHelper _output;

    public DeleteDirectoryTests(DirectoryServiceTestsFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task DeleteDirectoryFailsWhenDirectoryDoesNotExist()
    {
        var error = AssertExt.ResultError(await _fixture.DirectoryService.DeleteDirectoryAsync(-1));
        _output.WriteLine(error.ToString());
        Assert.Equal(DeleteErrorCodes.EntityNotFound, error.Code);
    }

    [Fact]
    public async Task DeleteDirectoryWorks()
    {
        var dirName = Guid.NewGuid().ToString("N");
        var dirModel = AssertExt.ResultOk(await _fixture.DirectoryService.CreateDirectoryAsync(0, dirName));
        
        var discoveredFses = AssertExt.ResultOk(await _fixture.FileSystemEntryRepository.GetFileSystemEntries(0));
        Assert.Contains(discoveredFses, fse => fse.IsDirectory && fse.FileSystemEntryId == dirModel.DirectoryId && string.Equals(fse.FileSystemEntryName, dirName));
        
        AssertExt.ResultOk(await _fixture.DirectoryService.DeleteDirectoryAsync(dirModel.DirectoryId));
        discoveredFses = AssertExt.ResultOk(await _fixture.FileSystemEntryRepository.GetFileSystemEntries(0));
        Assert.DoesNotContain(discoveredFses, fse => fse.IsDirectory && fse.FileSystemEntryId == dirModel.DirectoryId && string.Equals(fse.FileSystemEntryName, dirName));
    }
}