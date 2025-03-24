using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Interfaces;
using Directory = ReleaseBox.Core.Models.Directory;

namespace ReleaseBox.Core.Services;

public class DirectoryService : IDirectoryService
{
    private readonly IDirectoryRepository _directoryRepository;

    public DirectoryService(IDirectoryRepository directoryRepository)
    {
        _directoryRepository = directoryRepository;
    }

    public async Task<Result<Directory, Error<CreateErrorCodes>>> CreateDirectoryAsync(long parentDirectoryId, string directoryName)
    {
        return await _directoryRepository.CreateDirectoryAsync(parentDirectoryId, directoryName)
            .Map(directoryEntity => new Directory(directoryEntity.DirectoryId, directoryEntity.ParentDirectoryId, directoryEntity.DirectoryName)); 
    }

    public async Task<Result<long, Error<DeleteErrorCodes>>> DeleteDirectoryAsync(long directoryId)
    {
        return await _directoryRepository.DeleteDirectoryAsync(directoryId);
    }
}