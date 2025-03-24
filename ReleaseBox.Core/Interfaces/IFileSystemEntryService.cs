using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Models;

namespace ReleaseBox.Core.Interfaces;

public interface IFileSystemEntryService
{
    public Task<Result<IEnumerable<FileSystemEntry>, Error<GetErrorCodes>>> GetFileSystemEntries(long parentDirectoryId);
}