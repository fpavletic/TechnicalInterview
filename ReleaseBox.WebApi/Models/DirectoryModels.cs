namespace ReleaseBox.Models;

public readonly record struct DirectoryDto(long DirectoryId, long ParentDirectoryId, string DirectoryName);
    
public readonly record struct CreateDirectoryParametersDto
{
    public required long ParentDirectoryId { get; init; }
    public required string DirectoryName { get; init; }
}