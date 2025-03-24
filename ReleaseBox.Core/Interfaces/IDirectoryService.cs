using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using Directory = ReleaseBox.Core.Models.Directory;

namespace ReleaseBox.Core.Interfaces;

public interface IDirectoryService
{
    public Task<Result<Directory, Error<CreateErrorCodes>>> CreateDirectoryAsync(long parentDirectoryId, string directoryName);
    
    public Task<Result<long, Error<DeleteErrorCodes>>> DeleteDirectoryAsync(long directoryId);
}