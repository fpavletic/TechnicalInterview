using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Services;
using ReleaseBox.Data.Sqlite;
using ReleaseBox.Data.Sqlite.ConnectionDataProvider;
using ReleaseBox.Data.Sqlite.Repositories;

namespace ReleaseBox.Core.Test.FileServiceTests;

public class FileServiceTestsFixture : IDisposable
{
    internal FileRepository FileRepository { get; }
    internal DirectoryRepository DirectoryRepository { get; }
    internal FileSystemEntryRepository FileSystemEntryRepository { get; }
    
    internal FileService FileService { get; }

    public FileServiceTestsFixture()
    {
        var connectionDataProvider = new ConstantOnDiskConnectionDataProvider("./");
        using var databaseInitializer = new DatabaseInitializer(connectionDataProvider);
        databaseInitializer.Initialize();
        FileRepository = new FileRepository(connectionDataProvider);
        DirectoryRepository = new DirectoryRepository(connectionDataProvider);
        FileSystemEntryRepository = new FileSystemEntryRepository(connectionDataProvider);
        FileService = new FileService(FileRepository);
    }

    public void Dispose()
    {
        FileRepository.Dispose();
        DirectoryRepository.Dispose();
        FileSystemEntryRepository.Dispose();
    }
}

[CollectionDefinition(nameof(FileServiceTests))]
public class FileServiceTests : ICollectionFixture<FileServiceTestsFixture>{}