using ReleaseBox.Core.Data.ErrorCodes;
using Xunit.Abstractions;

namespace ReleaseBox.Core.Test.FileServiceTests;

[Collection(nameof(FileServiceTests))]
public class DeleteFileTests
{
    private readonly FileServiceTestsFixture _fixture;
    private readonly ITestOutputHelper _output;
    
    public DeleteFileTests(FileServiceTestsFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task DeleteFileFailsWhenFileDoesNotExist()
    {
        var error = AssertExt.ResultError(await _fixture.FileService.DeleteFileAsync(-1));
        _output.WriteLine(error.ToString());
        Assert.Equal(DeleteErrorCodes.EntityNotFound, error.Code);
    }

    [Fact]
    public async Task DeleteFileWorks()
    {
        var fileName = Guid.NewGuid().ToString("N");
        var fileModel = AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(0, fileName));
        
        var discoveredFses = AssertExt.ResultOk(await _fixture.FileSystemEntryRepository.GetFileSystemEntries(0));
        Assert.Contains(discoveredFses, fse => !fse.IsDirectory && fse.FileSystemEntryId == fileModel.FileId && string.Equals(fse.FileSystemEntryName, fileName));
        
        AssertExt.ResultOk(await _fixture.FileService.DeleteFileAsync(fileModel.FileId));
        discoveredFses = AssertExt.ResultOk(await _fixture.FileSystemEntryRepository.GetFileSystemEntries(0));
        Assert.DoesNotContain(discoveredFses, fse => !fse.IsDirectory && fse.FileSystemEntryId == fileModel.FileId && string.Equals(fse.FileSystemEntryName, fileName));
    }
}