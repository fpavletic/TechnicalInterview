using System.Data;
using System.Data.SQLite;
using ReleaseBox.Core.Data.Models;

using Directory = ReleaseBox.Data.Sqlite.Schema.Directory;
using File = ReleaseBox.Data.Sqlite.Schema.File;

namespace ReleaseBox.Data.Sqlite.Commands;

public class SearchForFilesInSubtree : IDisposable, IAsyncDisposable
{
    private const string RootDirectoryIdParameterName = "RootDirectoryIdParam";
    private const string FileNamePrefixParameterName = "FileNamePrefixParam";
    private const string MaxFileCountParameterName = "MaxFileCountParam";
    
    private const string SubtreeDirectoriesCte = "subtreeDirectories";   
    private const string Query = $"""
                                          WITH RECURSIVE 
                                          {SubtreeDirectoriesCte}({Directory.Id}) AS (
                                              VALUES(:{RootDirectoryIdParameterName})
                                              UNION
                                              SELECT {Directory.TableName}.{Directory.Id} 
                                              FROM {Directory.TableName}
                                              INNER JOIN {SubtreeDirectoriesCte}
                                              ON {Directory.TableName}.{Directory.ParentDirectoryId} = {SubtreeDirectoriesCte}.{Directory.Id}
                                          )

                                          SELECT {File.TableName}.{File.Id}, {File.TableName}.{File.ParentDirectoryId}, {File.TableName}.{File.Name}
                                          FROM {File.TableName}
                                          INNER JOIN {SubtreeDirectoriesCte}
                                          ON {File.TableName}.{File.ParentDirectoryId} = {SubtreeDirectoriesCte}.{Directory.Id}
                                          WHERE {File.TableName}.{File.Name} LIKE :{FileNamePrefixParameterName}
                                          LIMIT :{MaxFileCountParameterName}
                                          """;
    
    private readonly SQLiteCommand _command;
    private readonly SQLiteParameter _rootDirectoryIdParameter;
    private readonly SQLiteParameter _fileNamePrefixParameter;
    private readonly SQLiteParameter _maxFileCountParameter;
    
    public SearchForFilesInSubtree(SQLiteTransaction transaction)
    {
        _command = new SQLiteCommand(Query, transaction.Connection, transaction);
        _rootDirectoryIdParameter = new SQLiteParameter(RootDirectoryIdParameterName, DbType.Int64);
        _command.Parameters.Add(_rootDirectoryIdParameter);
        _fileNamePrefixParameter = new SQLiteParameter(FileNamePrefixParameterName, DbType.String);
        _command.Parameters.Add(_fileNamePrefixParameter);
        _maxFileCountParameter = new SQLiteParameter(MaxFileCountParameterName, DbType.Int32);
        _command.Parameters.Add(_maxFileCountParameter);
    }

    public async Task<IReadOnlyCollection<FileEntity>> Execute(long rootDirectoryId, string fileNamePrefix, int maxFileCount)
    {
        _rootDirectoryIdParameter.Value = rootDirectoryId;
        _fileNamePrefixParameter.Value = $"{fileNamePrefix}%";
        _maxFileCountParameter.Value = maxFileCount;
        
        var foundFiles = new List<FileEntity>();
        
        await using var sqliteDataReader = await _command.ExecuteReaderAsync();
        while (await sqliteDataReader.ReadAsync())
        {
            foundFiles.Add(new FileEntity(
                sqliteDataReader.GetInt64(sqliteDataReader.GetOrdinal(File.Id)),
                sqliteDataReader.GetInt64(sqliteDataReader.GetOrdinal(File.ParentDirectoryId)),
                sqliteDataReader.GetString(sqliteDataReader.GetOrdinal(File.Name))
                ));                                                    
        }
        return foundFiles;
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