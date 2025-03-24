namespace ReleaseBox.Core.Data.Models;

public readonly record struct FileSystemEntryEntity(long FileSystemEntryId, long ParentDirectoryId, string FileSystemEntryName, bool IsDirectory);