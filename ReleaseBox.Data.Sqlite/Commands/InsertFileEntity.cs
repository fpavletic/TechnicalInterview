using System.Data;
using System.Data.SQLite;
using File = ReleaseBox.Data.Sqlite.Schema.File;

namespace ReleaseBox.Data.Sqlite.Commands;

public class InsertFileEntity : IDisposable, IAsyncDisposable
{
    private const string ParentDirectoryIdParameterName = "ParentDirectoryIdParam";
    private const string NameParameterName = "FileNameParam";
    private const string Query = $"""
INSERT INTO {File.TableName} ({File.ParentDirectoryId}, {File.Name}) 
    VALUES (:{ParentDirectoryIdParameterName}, :{NameParameterName})
    RETURNING {File.Id}
""";
    
    private readonly SQLiteCommand _command;
    private readonly SQLiteParameter _parentDirectoryIdParameter;
    private readonly SQLiteParameter _nameParameter;
    
    public InsertFileEntity(SQLiteTransaction transaction)
    {
        _command = new SQLiteCommand(Query, transaction.Connection, transaction);
        _parentDirectoryIdParameter = new SQLiteParameter(ParentDirectoryIdParameterName, DbType.Int64);
        _command.Parameters.Add(_parentDirectoryIdParameter);
        _nameParameter = new SQLiteParameter(NameParameterName, DbType.String);
        _command.Parameters.Add(_nameParameter);
    }

    public async Task<long> Execute(long parentDirectoryId, string fileName)
    {
        _parentDirectoryIdParameter.Value = parentDirectoryId;
        _nameParameter.Value = fileName;
        await using var sqliteDataReader = await _command.ExecuteReaderAsync();
        return await sqliteDataReader.ReadAsync()
            ? sqliteDataReader.GetInt64(0)
            : throw new Exception("We inserted a row, but failed to get back an id?"); //Should never happen
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