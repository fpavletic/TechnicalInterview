namespace ReleaseBox.Models;

public readonly record struct FileSystemEntryDto(long FileSystemEntryId, long ParentDirectoryId, string FileSystemEntryName, bool IsDirectory);