using System.Data.SQLite;
using ReleaseBox.Core.Data.Interface;

using Directory = ReleaseBox.Data.Sqlite.Schema.Directory;
using File = ReleaseBox.Data.Sqlite.Schema.File;

namespace ReleaseBox.Data.Sqlite;

public class DatabaseInitializer : IDisposable                                                                                                  
{
    private readonly Lazy<SQLiteConnection> _connection;

    public DatabaseInitializer(IConnectionDataProvider connectionDataProvider)
    {
        _connection = new Lazy<SQLiteConnection>(() =>
        {
            var connection = new SQLiteConnection(connectionDataProvider.GetConnectionString());
            connection.Open();
            return connection;
        });
    }

    public void Initialize()
    {
        using var transaction = _connection.Value.BeginTransaction();
        using var createDirectoryTableCommand = new SQLiteCommand(CreateDirectoryTable, _connection.Value, transaction);
        createDirectoryTableCommand.ExecuteNonQuery();
        using var createFileTableCommand = new SQLiteCommand(CreateFileTable, _connection.Value, transaction);
        createFileTableCommand.ExecuteNonQuery();
        using var getRootDirectoryCommand = new SQLiteCommand(GetRootDir, _connection.Value, transaction);
        var rootDirReader = getRootDirectoryCommand.ExecuteReader();
        if (!rootDirReader.Read())
        {   
            using var insertRootDirectoryCommand = new SQLiteCommand(InsertRootDir, _connection.Value, transaction);
            insertRootDirectoryCommand.ExecuteNonQuery();    
        }
        rootDirReader.Close();
        rootDirReader.Dispose();
        transaction.Commit();
    }
    
    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Close();
            _connection.Value.Dispose();
        }
    }
    
    private const string CreateDirectoryTable = $"""
                                                  CREATE TABLE IF NOT EXISTS {Directory.TableName}(
                                                    {Directory.Id} INTEGER PRIMARY KEY,
                                                    {Directory.ParentDirectoryId} INTEGER,
                                                    {Directory.Name} TEXT NOT NULL CHECK(TRIM({Directory.Name}) <> ''),
                                                    UNIQUE({Directory.ParentDirectoryId}, {Directory.Name}),
                                                    FOREIGN KEY ({Directory.ParentDirectoryId}) REFERENCES {Directory.TableName}({Directory.Id}) ON DELETE CASCADE
                                                  );                                                 
                                                  CREATE INDEX IF NOT EXISTS {Directory.TableName}_{Directory.ParentDirectoryId}_index ON {Directory.TableName}({Directory.ParentDirectoryId})
                                                  """;

    private const string CreateFileTable = $"""
                                            CREATE TABLE IF NOT EXISTS {File.TableName}(
                                                {File.Id} INTEGER PRIMARY KEY,
                                                {File.ParentDirectoryId} INTEGER,
                                                {File.Name} TEXT NOT NULL CHECK(TRIM({File.Name}) <> ''),
                                                UNIQUE({File.ParentDirectoryId}, {File.Name}),
                                                FOREIGN KEY ({File.ParentDirectoryId}) REFERENCES {Directory.TableName}({Directory.Id}) ON DELETE CASCADE
                                            );
                                            CREATE INDEX IF NOT EXISTS {File.TableName}_{File.ParentDirectoryId}_index ON {File.TableName}({File.ParentDirectoryId})
                                            """;

    private const string GetRootDir = $"""
                                        SELECT {Directory.Id} FROM {Directory.TableName}
                                        """;
    
    private const string InsertRootDir = $"""
                                          INSERT INTO {Directory.TableName}({Directory.Id}, {Directory.ParentDirectoryId}, {Directory.Name})
                                          VALUES(0, NULL, '~')
                                          """;
}