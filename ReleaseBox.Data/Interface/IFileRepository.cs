using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Models;

namespace ReleaseBox.Core.Data.Interface;

public interface IFileRepository
{
    public Task<Result<FileEntity, Error<CreateErrorCodes>>> CreateFileAsync(long parentDirectoryId, string fileName);
    
    public Task<Result<IReadOnlyCollection<FileEntity>, Error<GetErrorCodes>>> SearchFilesAsync(long rootDirectoryId, string fileNamePrefix, int maxFileCount);
    
    public Task<Result<long, Error<DeleteErrorCodes>>> DeleteFileAsync(long fileId);
}