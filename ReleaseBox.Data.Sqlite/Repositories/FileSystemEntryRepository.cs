using System.Data.SQLite;
using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Data.Models;
using ReleaseBox.Data.Sqlite.Commands;

namespace ReleaseBox.Data.Sqlite.Repositories;

public class FileSystemEntryRepository : IFileSystemEntryRepository, IDisposable
{
    private readonly Lazy<SQLiteConnection> _connection;

    public FileSystemEntryRepository(IConnectionDataProvider connectionDataProvider)
    {
        _connection = new Lazy<SQLiteConnection>(() =>
        {
            var connection = new SQLiteConnection(connectionDataProvider.GetConnectionString());
            connection.Open();
            connection.SetExtendedResultCodes(true);
            return connection;
        });
    }
    
    public async Task<Result<IReadOnlyCollection<FileSystemEntryEntity>, Error<GetErrorCodes>>> GetFileSystemEntries(long parentDirectoryId)
    {
        SQLiteTransaction? transaction = null;
        try
        {
            transaction = (SQLiteTransaction)await _connection.Value.BeginTransactionAsync();
            await using var command = new GetFileSystemEntriesByParentDirectoryId(transaction);
            var foundFileSystemEntries = await command.Execute(parentDirectoryId);
            await transaction.CommitAsync();
            return Result<IReadOnlyCollection<FileSystemEntryEntity>, Error<GetErrorCodes>>.Ok(foundFileSystemEntries);
        }
        catch (Exception e)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<GetErrorCodes>(GetErrorCodes.UnknownError, 
                $"An unexpected error occurred while searching for files", e);
        }
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Dispose();
        }
    }
}