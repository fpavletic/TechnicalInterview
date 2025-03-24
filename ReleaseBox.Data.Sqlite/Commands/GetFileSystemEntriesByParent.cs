using System.Data;
using System.Data.SQLite;
using ReleaseBox.Core.Data.Models;
using Directory = ReleaseBox.Data.Sqlite.Schema.Directory;
using File = ReleaseBox.Data.Sqlite.Schema.File; 

namespace ReleaseBox.Data.Sqlite.Commands;

public class GetFileSystemEntriesByParentDirectoryId : IDisposable, IAsyncDisposable
{
    private const string IdColumnName = "Id";
    private const string NameColumnName = "Name";
    private const string IsDirectoryColumnName = "IsDirectory";
    private const string ParentDirectoryIdParameterName = "ParentDirectoryIdParam";
    private const string Query =
        $"""
SELECT {Directory.TableName}.{Directory.Id} as {IdColumnName}, {Directory.TableName}.{Directory.Name} as {NameColumnName}, 1 AS {IsDirectoryColumnName} 
FROM {Directory.TableName} 
WHERE {Directory.TableName}.{Directory.ParentDirectoryId} = :{ParentDirectoryIdParameterName}

UNION

SELECT {File.TableName}.{File.Id} as {IdColumnName}, {File.TableName}.{File.Name} as {NameColumnName}, 0 AS {IsDirectoryColumnName} 
FROM {File.TableName} 
WHERE {File.TableName}.{File.ParentDirectoryId} = :{ParentDirectoryIdParameterName}
""";
    
    private readonly SQLiteCommand _command;
    private readonly SQLiteParameter _idParameter;
    
    public GetFileSystemEntriesByParentDirectoryId(SQLiteTransaction transaction)
    {
        _command = new SQLiteCommand(Query, transaction.Connection, transaction);
        _idParameter = new SQLiteParameter(ParentDirectoryIdParameterName, DbType.Int64);
        _command.Parameters.Add(_idParameter);
    }

    public async Task<IReadOnlyCollection<FileSystemEntryEntity>> Execute(long parentDirectoryId)
    {
        _idParameter.Value = parentDirectoryId;
        var foundFileSystemEntries = new List<FileSystemEntryEntity>();
        await using var sqliteDataReader = await _command.ExecuteReaderAsync();
        while (await sqliteDataReader.ReadAsync())
        {
            foundFileSystemEntries.Add(new FileSystemEntryEntity(
                sqliteDataReader.GetInt64(sqliteDataReader.GetOrdinal(IdColumnName)),
                parentDirectoryId,
                sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(NameColumnName)),
                sqliteDataReader.GetBoolean(sqliteDataReader.GetOrdinal(IsDirectoryColumnName))));
        }
        return foundFileSystemEntries;
    }

    public void Dispose()
    {
        _command.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
    }
}