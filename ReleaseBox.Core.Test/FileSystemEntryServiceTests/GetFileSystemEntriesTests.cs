using ReleaseBox.Core.Models;

namespace ReleaseBox.Core.Test.FileSystemEntryServiceTests;

[Collection(nameof(FileSystemEntryServiceTests))]
public class GetFileSystemEntriesTests
{
    private readonly FileSystemEntryServiceFixture _fixture;

    public GetFileSystemEntriesTests(FileSystemEntryServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetFileSystemEntriesReturnsAnEmptyCollectionWhenProvidedANonExistentRoot()
    {
        var foundFileSystemEntries = AssertExt.ResultOk(await _fixture.FileSystemEntryService.GetFileSystemEntries(-1));
        Assert.Empty(foundFileSystemEntries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task GetFileSystemEntriesReturnsAllExpectedFileEntries(int fileCount)
    {
        var parentDirName = Guid.NewGuid().ToString("N");
        var parentDir = AssertExt.ResultOk(await _fixture.DirectoryRepository.CreateDirectoryAsync(0, parentDirName));

        var validFileSystemEntries = new List<FileSystemEntry>();
        
        for (int i = 0; i < fileCount; i++)
        {
            var fileName = Guid.NewGuid().ToString("N");
            var fileModel = AssertExt.ResultOk(await _fixture.FileRepository.CreateFileAsync(parentDir.DirectoryId, fileName));
            validFileSystemEntries.Add(new FileSystemEntry(fileModel.FileId, fileModel.ParentDirectoryId, fileModel.FileName, false));
         
            var dirName = Guid.NewGuid().ToString("N");
            var dirModel = AssertExt.ResultOk(await _fixture.DirectoryRepository.CreateDirectoryAsync(parentDir.DirectoryId, dirName));
            validFileSystemEntries.Add(new FileSystemEntry(dirModel.DirectoryId, dirModel.ParentDirectoryId, dirModel.DirectoryName, true));
        }
        
        var foundFileSystemEntries = AssertExt.ResultOk(await _fixture.FileSystemEntryService.GetFileSystemEntries(parentDir.DirectoryId)).ToArray();
        Assert.Equal(validFileSystemEntries.Count, foundFileSystemEntries.Length);
        Assert.All(validFileSystemEntries, fse => Assert.Contains(fse, foundFileSystemEntries));
    }

    [Fact]
    public async Task OnlyReturnsDirectChildren_DoesNotRecurse()
    {
        var parentDirName = Guid.NewGuid().ToString("N");
        var parentDir = AssertExt.ResultOk(await _fixture.DirectoryRepository.CreateDirectoryAsync(0, parentDirName));
        var childDirName = Guid.NewGuid().ToString("N");
        var childDir = AssertExt.ResultOk(await _fixture.DirectoryRepository.CreateDirectoryAsync(parentDir.DirectoryId, childDirName));
        
        var foundFileSystemEntries = AssertExt.ResultOk(await _fixture.FileSystemEntryService.GetFileSystemEntries(0)).ToArray();
        Assert.Contains(new FileSystemEntry(parentDir.DirectoryId, parentDir.ParentDirectoryId, parentDir.DirectoryName, true), foundFileSystemEntries);
        Assert.DoesNotContain(new FileSystemEntry(childDir.DirectoryId, childDir.ParentDirectoryId, childDir.DirectoryName, true), foundFileSystemEntries);
        
    }

}