namespace ReleaseBox.Core.Models;

public readonly record struct Directory(long DirectoryId, long ParentDirectoryId, string DirectoryName);
