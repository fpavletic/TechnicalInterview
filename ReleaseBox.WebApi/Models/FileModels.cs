namespace ReleaseBox.Models;

public readonly record struct FileDto(long FileId, long ParentDirectoryId, string FileName);

public readonly record struct CreateFileParametersDto
{
    public required long ParentDirectoryId { get; init; }
    
    public required string FileName { get; init; }
}

public record FileSearchParametersDto()
{
    public long RootDirectoryId { get; init; } = 1;

    public string FileNamePrefix { get; init; } = string.Empty;
}