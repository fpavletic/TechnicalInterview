using Common.Error;
using Common.Expect;
using ReleaseBox.Core.Data.ErrorCodes;
using ReleaseBox.Core.Data.Interface;
using ReleaseBox.Core.Interfaces;
using File = ReleaseBox.Core.Models.File;

namespace ReleaseBox.Core.Services;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    
    public FileService(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    public async Task<Result<File, Error<CreateErrorCodes>>> CreateFileAsync(long parentDirectoryId, string fileName)
    {
        return await _fileRepository.CreateFileAsync(parentDirectoryId, fileName)
            .Map(fileEntity => new File(fileEntity.FileId, fileEntity.ParentDirectoryId, fileEntity.FileName));
    }

    public async Task<Result<IEnumerable<File>, Error<GetErrorCodes>>> SearchFiles(long rootDirectoryId, string fileNamePrefix, int maxFileCount)
    {
        return await _fileRepository.SearchFilesAsync(rootDirectoryId, fileNamePrefix, maxFileCount)
            .Map(files => files.Select(file => new File(file.FileId, file.ParentDirectoryId, file.FileName)));
    }

    public async Task<Result<long, Error<DeleteErrorCodes>>> DeleteFileAsync(long fileId)
    {
        return await _fileRepository.DeleteFileAsync(fileId);
    }
}