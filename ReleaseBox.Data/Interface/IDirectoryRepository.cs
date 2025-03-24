using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Models;

namespace ReleaseBox.Core.Data.Interface;

public interface IDirectoryRepository
{
    public Task<Result<DirectoryEntity, Error<CreateErrorCodes>>> CreateDirectoryAsync(long parentDirectoryId, string directoryName);
    
    public Task<Result<long, Error<DeleteErrorCodes>>> DeleteDirectoryAsync(long directoryId);
}