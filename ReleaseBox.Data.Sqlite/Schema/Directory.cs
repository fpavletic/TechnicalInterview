namespace ReleaseBox.Data.Sqlite.Schema;

internal static class Directory
{
    internal const string TableName = "directory";
    internal const string Id = $"{TableName}_id";
    internal const string Name = $"{TableName}_name";
    internal const string ParentDirectoryId = "parent_directory_id";
}