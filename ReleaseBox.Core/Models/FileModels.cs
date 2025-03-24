namespace ReleaseBox.Core.Models;

public readonly record struct File(long FileId, long ParentDirectoryId, string FileName);

