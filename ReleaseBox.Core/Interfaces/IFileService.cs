using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using File = ReleaseBox.Core.Models.File;

namespace ReleaseBox.Core.Interfaces;

public interface IFileService
{
    public Task<Result<File, Error<CreateErrorCodes>>> CreateFileAsync(long parentDirectoryId, string fileName);

    public Task<Result<IEnumerable<File>, Error<GetErrorCodes>>> SearchFiles(long rootDirectoryId, string fileNamePrefix, int maxFileCount = int.MaxValue);
    
    public Task<Result<long, Error<DeleteErrorCodes>>> DeleteFileAsync(long fileId);
}