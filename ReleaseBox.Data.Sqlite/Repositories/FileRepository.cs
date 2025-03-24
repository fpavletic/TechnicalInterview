using System.Data.SQLite;
using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Data.Models;
using ReleaseBox.Data.Sqlite.Commands;

using File = ReleaseBox.Data.Sqlite.Schema.File;

namespace ReleaseBox.Data.Sqlite.Repositories;

public class FileRepository : IFileRepository, IDisposable
{
    private readonly Lazy<SQLiteConnection> _connection;

    public FileRepository(IConnectionDataProvider connectionDataProvider)
    {
        _connection = new Lazy<SQLiteConnection>(() =>
        {
            var connection = new SQLiteConnection(connectionDataProvider.GetConnectionString());
            connection.Open();
            connection.SetExtendedResultCodes(true);
            return connection;
        });
    }
    
    public async Task<Result<FileEntity, Error<CreateErrorCodes>>> CreateFileAsync(long parentDirectoryId, string fileName)
    {
        SQLiteTransaction? transaction = null;
        try
        {
            transaction = (SQLiteTransaction)await _connection.Value.BeginTransactionAsync();
            await using var command = new InsertFileEntity(transaction);
            var fileId = await command.Execute(parentDirectoryId, fileName);
            await transaction.CommitAsync();
            return new FileEntity(fileId, parentDirectoryId, fileName);
        }
        catch (SQLiteException e) when (e.ResultCode == SQLiteErrorCode.Constraint_ForeignKey)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.ParentNotFound, $"Attempted to create a file with a non-existent parent directory. Parent directory id: '{parentDirectoryId}'", e);
        }
        catch (SQLiteException e) when (e.ResultCode == SQLiteErrorCode.Constraint_Unique)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.Duplicate, $"Attempted to create a duplicate file. Parent directory id: '{parentDirectoryId}'; File name: '{fileName}'", e);
        }        
        catch (SQLiteException e) when (e.ResultCode is SQLiteErrorCode.Constraint_Check or SQLiteErrorCode.Constraint_NotNull)
        {
            if (transaction != null) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.Invalid,
                $"Attempted to create a file with an invalid name. File name: '{fileName}'",
                e);
        }
        catch (Exception e)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<CreateErrorCodes>(CreateErrorCodes.UnknownError, "An unexpected error occurred while creating a file", e);            
        }
        finally
        {
            if (transaction != null) await transaction.DisposeAsync();
        }
    }
    
    public async Task<Result<IReadOnlyCollection<FileEntity>, Error<GetErrorCodes>>> SearchFilesAsync(long rootDirectoryId, string fileNamePrefix, int maxFileCount)
    {
        SQLiteTransaction? transaction = null;
        try
        {
            transaction = (SQLiteTransaction)await _connection.Value.BeginTransactionAsync();
            await using var command = new SearchForFilesInSubtree(transaction);
            var foundFiles = await command.Execute(rootDirectoryId, fileNamePrefix, maxFileCount);
            await transaction.CommitAsync();
            return Result<IReadOnlyCollection<FileEntity>, Error<GetErrorCodes>>.Ok(foundFiles);
        }
        catch (Exception e)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<GetErrorCodes>(GetErrorCodes.UnknownError, 
                $"An unexpected error occurred while searching for files", e);
        }
    }

    public async Task<Result<long, Error<DeleteErrorCodes>>> DeleteFileAsync(long fileId)
    {
        SQLiteTransaction? transaction = null;
        try
        {
            transaction = (SQLiteTransaction)await _connection.Value.BeginTransactionAsync();
            await using var command = new DeleteById(transaction, File.TableName, File.Id);
            var rowsAffected = await command.Execute(fileId);

            if (rowsAffected == 1)
            {
                await transaction.CommitAsync();
                return fileId;
            }

            await transaction.RollbackAsync();

            return rowsAffected == 0
                ? new Error<DeleteErrorCodes>(DeleteErrorCodes.EntityNotFound, $"Attempted to delete a non-existent file. File id: '{fileId}'")
                : new Error<DeleteErrorCodes>(DeleteErrorCodes.MultipleMatches,
                    "Multiple entities match the provided id?"); //Should never happen   
        }
        catch (Exception e)
        {
            if ( transaction != null ) await transaction.RollbackAsync();
            return new Error<DeleteErrorCodes>(DeleteErrorCodes.UnknownError, $"An unexpected error occurred while deleting a file", e);            
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