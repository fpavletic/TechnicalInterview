namespace ReleaseBox.Data.Sqlite.Schema;

internal static class File
{
    internal const string TableName = "file";
    
    internal const string Id = $"{TableName}_id";
    internal const string Name = $"{TableName}_name";
    internal const string ParentDirectoryId = "parent_directory_id";
    
    internal const string Columns = $"{TableName}.{Id}, {TableName}.{ParentDirectoryId}, {TableName}.{Name}";
}