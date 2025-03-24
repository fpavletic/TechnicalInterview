using ReleaseBox.Core.Services;
using ReleaseBox.Data.Sqlite;
using ReleaseBox.Data.Sqlite.ConnectionDataProvider;
using ReleaseBox.Data.Sqlite.Repositories;

namespace ReleaseBox.Core.Test.FileSystemEntryServiceTests;

public class FileSystemEntryServiceFixture : IDisposable
{
    internal FileRepository FileRepository { get; }
    internal DirectoryRepository DirectoryRepository { get; }
    internal FileSystemEntryRepository FileSystemEntryRepository { get; }
    
    internal FileSystemEntryService FileSystemEntryService { get; }

    public FileSystemEntryServiceFixture()
    {
        var connectionDataProvider = new ConstantOnDiskConnectionDataProvider("./");
        using var databaseInitializer = new DatabaseInitializer(connectionDataProvider);
        databaseInitializer.Initialize();
        FileRepository = new FileRepository(connectionDataProvider);
        DirectoryRepository = new DirectoryRepository(connectionDataProvider);
        FileSystemEntryRepository = new FileSystemEntryRepository(connectionDataProvider);
        FileSystemEntryService = new FileSystemEntryService(FileSystemEntryRepository);
    }

    public void Dispose()
    {
        FileRepository.Dispose();
        DirectoryRepository.Dispose();
        FileSystemEntryRepository.Dispose();
    }
}

[CollectionDefinition(nameof(FileSystemEntryServiceTests))]
public class FileSystemEntryServiceTests : ICollectionFixture<FileSystemEntryServiceFixture>
{
}