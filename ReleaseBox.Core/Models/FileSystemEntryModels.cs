namespace ReleaseBox.Core.Models;

public readonly record struct FileSystemEntry(long FileSystemEntryId, long ParentDirectoryId, string FileSystemEntryName, bool IsDirectory);