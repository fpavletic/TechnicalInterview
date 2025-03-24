using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Models;

namespace ReleaseBox.Core.Data.Interface;

public interface IFileSystemEntryRepository
{
    public Task<Result<IReadOnlyCollection<FileSystemEntryEntity>, Error<GetErrorCodes>>> GetFileSystemEntries(long parentDirectoryId);
}