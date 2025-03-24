using System.Data.SQLite;
using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Data.Models;
using ReleaseBox.Data.Sqlite.Commands;
using Directory = ReleaseBox.Data.Sqlite.Schema.Directory;

namespace ReleaseBox.Data.Sqlite.Repositories;

public class DirectoryRepository : IDirectoryRepository, IDisposable
{
    private readonly Lazy<SQLiteConnection> _connection;

    public DirectoryRepository(IConnectionDataProvider connectionDataProvider)
    {
        _connection = new Lazy<SQLiteConnection>(() =>
        {
            var connection = new SQLiteConnection(connectionDataProvider.GetConnectionString());
            connection.Open();
            connection.SetExtendedResultCodes(true);
            return connection;
        });
    }
    
    public async Task<Result<DirectoryEntity, Error<CreateErrorCodes>>> CreateDirectoryAsync(long parentDirectoryId, string directoryName)
    {
        SQLiteTransaction? transaction = null;
        try
        {
            transaction = (SQLiteTransaction)await _connection.Value.BeginTransactionAsync();
            await using var command = new InsertDirectoryEntity(transaction);
            var directoryId = await command.Execute(parentDirectoryId, directoryName);
            await transaction.CommitAsync();
            return new DirectoryEntity(directoryId, parentDirectoryId, directoryName);
        }
        catch (SQLiteException e) when (e.ResultCode == SQLiteErrorCode.Constraint_ForeignKey)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.ParentNotFound,
                $"Attempted to create a directory with a non-existent parent directory. Parent directory id: '{parentDirectoryId}'",
                e);
        }
        catch (SQLiteException e) when (e.ResultCode == SQLiteErrorCode.Constraint_Unique)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.Duplicate,
                $"Attempted to create a duplicate directory. Parent directory id: '{parentDirectoryId}'; Directory name: '{directoryName}'",
                e);
        }
        catch (SQLiteException e) when (e.ResultCode is SQLiteErrorCode.Constraint_Check or SQLiteErrorCode.Constraint_NotNull)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.Invalid,
                $"Attempted to create a directory with an invalid name. Directory name: '{directoryName}'",
                e);
        }
        catch (Exception e)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.UnknownError,
                "An unexpected error occurred while creating a directory", e);
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }

    public async Task<Result<long, Error<DeleteErrorCodes>>> DeleteDirectoryAsync(long directoryId)
    {
        SQLiteTransaction? transaction = null;
        try
        {
            transaction = (SQLiteTransaction)await _connection.Value.BeginTransactionAsync();
            await using var command = new DeleteById(transaction, Directory.TableName, Directory.Id, Directory.ParentDirectoryId);
            var rowsAffected = await command.Execute(directoryId);

            if (rowsAffected == 1)
            {
                await transaction.CommitAsync();
                return directoryId;
            }

            await transaction.RollbackAsync();

            return rowsAffected == 0
                ? new Error<DeleteErrorCodes>(DeleteErrorCodes.EntityNotFound, $"Attempted to delete a non-existent directory. Directory id: '{directoryId}'")
                : new Error<DeleteErrorCodes>(DeleteErrorCodes.UnknownError,
                    "Multiple entities match the provided id?"); //Should never happen   
        }
        catch (Exception e)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<DeleteErrorCodes>(DeleteErrorCodes.UnknownError, $"An unexpected error occurred while deleting a directory", e);            
        }
    }

    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            _connection.Value.Close();
            _connection.Value.Dispose();
        }
    }
}