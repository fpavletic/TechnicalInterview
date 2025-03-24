using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Services;
using ReleaseBox.Data.Sqlite;
using ReleaseBox.Data.Sqlite.ConnectionDataProvider;
using ReleaseBox.Data.Sqlite.Repositories;

namespace ReleaseBox.Core.Test.DirectoryServiceTests;

public class DirectoryServiceTestsFixture : IDisposable
{
    internal DirectoryRepository DirectoryRepository { get; } 
    
    internal FileSystemEntryRepository FileSystemEntryRepository { get; }
    
    internal DirectoryService DirectoryService { get; }

    public DirectoryServiceTestsFixture()
    {
        var connectionDataProvider = new ConstantOnDiskConnectionDataProvider("./");
        using var databaseInitializer = new DatabaseInitializer(connectionDataProvider);
        databaseInitializer.Initialize();
        DirectoryRepository = new DirectoryRepository(connectionDataProvider);
        FileSystemEntryRepository = new FileSystemEntryRepository(connectionDataProvider);
        DirectoryService = new DirectoryService(DirectoryRepository);
    }

    public void Dispose()
    {
        DirectoryRepository.Dispose();
        FileSystemEntryRepository.Dispose();
    }
}

[CollectionDefinition(nameof(DirectoryServiceTests))]
public class DirectoryServiceTests : ICollectionFixture<DirectoryServiceTestsFixture>{}