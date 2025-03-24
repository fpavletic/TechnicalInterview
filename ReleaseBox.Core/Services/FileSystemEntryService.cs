using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Interfaces;
using ReleaseBox.Core.Models;

namespace ReleaseBox.Core.Services;

public class FileSystemEntryService : IFileSystemEntryService
{
    private readonly IFileSystemEntryRepository _fileSystemEntryRepository;

    public FileSystemEntryService(IFileSystemEntryRepository fileSystemEntryRepository)
    {
        _fileSystemEntryRepository = fileSystemEntryRepository;
    }

    public async Task<Result<IEnumerable<FileSystemEntry>, Error<GetErrorCodes>>> GetFileSystemEntries(long parentDirectoryId)
    {
        return await _fileSystemEntryRepository.GetFileSystemEntries(parentDirectoryId)
            .Map(fses => fses.Select(fse => new FileSystemEntry(fse.FileSystemEntryId, fse.ParentDirectoryId, fse.FileSystemEntryName, fse.IsDirectory)));
    }
}