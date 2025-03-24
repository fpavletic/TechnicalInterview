using System.Data;
using System.Data.SQLite;
using Directory = ReleaseBox.Data.Sqlite.Schema.Directory;

namespace ReleaseBox.Data.Sqlite.Commands;

public class DeleteById : IDisposable, IAsyncDisposable
{
    private const string IdParameterName = "EntityIdParam";
    private const string QueryTemplate = $"DELETE FROM {{0}} WHERE {{1}} = :{IdParameterName} AND {{2}} IS NOT NULL";
    
    private readonly SQLiteCommand _command;
    private readonly SQLiteParameter _idParameter;
    
    public DeleteById(SQLiteTransaction transaction, string tableName, string idColumn, string parentIdColumn)
    {
        _command = new SQLiteCommand(string.Format(QueryTemplate, tableName, idColumn, parentIdColumn), transaction.Connection, transaction);
        _idParameter = new SQLiteParameter(IdParameterName, DbType.Int64);
        _command.Parameters.Add(_idParameter);
    }

    public async Task<long> Execute(long entityId)
    {
        _idParameter.Value = entityId;
        return await _command.ExecuteNonQueryAsync();
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