namespace ReleaseBox.Core.Test.FileServiceTests;

[Collection(nameof(FileServiceTests))]
public class SearchFilesTests
{
    private readonly FileServiceTestsFixture _fixture;

    public SearchFilesTests(FileServiceTestsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SearchFilesOnlyReturnsFilesFromTheTargetSubtree()
    {
        var dirName = Guid.NewGuid().ToString("N");
        var dirModel = AssertExt.ResultOk(await _fixture.DirectoryRepository.CreateDirectoryAsync(0, dirName));
        
        var fileName = Guid.NewGuid().ToString("N");
        var validFileModel = AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(dirModel.DirectoryId, fileName));
        var invalidFileModel = AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(0, fileName));
        
        var foundFiles = AssertExt.ResultOk(await _fixture.FileService.SearchFiles(dirModel.DirectoryId, string.Empty, int.MaxValue)).ToArray();
        Assert.Contains(foundFiles, f => f.FileId == validFileModel.FileId);
        Assert.DoesNotContain(foundFiles, f => f.FileId == invalidFileModel.FileId);
    }
    
    [Fact]
    public async Task SearchFilesReturnsAnEmptyCollectionWhenProvidedANonExistentRoot()
    {
        var foundFiles = AssertExt.ResultOk(await _fixture.FileService.SearchFiles(-1, string.Empty, int.MaxValue));
        Assert.Empty(foundFiles);
    }

    [Fact]
    public async Task SearchFilesOnlyReturnsFilesWithTheProvidedPrefix()
    {
        
        var validFileName = Guid.NewGuid().ToString("N");
        var validFileModel = AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(0, validFileName));
        var invalidFileName = Guid.NewGuid().ToString("N");
        var invalidFileModel = AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(0, invalidFileName));

        var indexOfFirstMismatchedCharacter = Enumerable.Range(0, validFileName.Length)
            .First(i => validFileName[i] != invalidFileName[i]);

        var foundFiles = AssertExt.ResultOk(await _fixture.FileService.SearchFiles(0, validFileName[..(indexOfFirstMismatchedCharacter+1)], int.MaxValue))
            .ToArray();
        
        Assert.Contains(foundFiles, f => f.FileId == validFileModel.FileId);
        Assert.DoesNotContain(foundFiles, f => f.FileId == invalidFileModel.FileId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    public async Task SearchFilesReturnsAllFilesInSubtreeWhenNotProvidedAPrefix(int fileCount)
    {
        var dirName = Guid.NewGuid().ToString("N");
        var dirModel = AssertExt.ResultOk(await _fixture.DirectoryRepository.CreateDirectoryAsync(0, dirName));
        
        var validFiles = new List<ReleaseBox.Core.Models.File>(fileCount);
        for (int i = 0; i < fileCount; i++)
        {
            validFiles.Add(AssertExt.ResultOk(await _fixture.FileService.CreateFileAsync(dirModel.DirectoryId, Guid.NewGuid().ToString("N"))));
        }
        
        var foundFiles = AssertExt.ResultOk(await _fixture.FileService.SearchFiles(dirModel.DirectoryId, string.Empty, int.MaxValue)).ToArray();
        Assert.Equal(validFiles.Count, foundFiles.Length);
        Assert.All(validFiles, f => Assert.Contains(f, foundFiles));
    }
}