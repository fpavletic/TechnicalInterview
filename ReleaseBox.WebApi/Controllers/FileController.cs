using Common.Expect;
using Microsoft.AspNetCore.Mvc;
using ReleaseBox.Core.Interfaces;
using ReleaseBox.Models;
using ReleaseBox.Util;

namespace ReleaseBox.Controllers;

[ApiController]
[Route("api/file")]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<FileController> _logger;

    public FileController(IFileService fileService, ILogger<FileController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<FileDto>> CreateFile([FromBody] CreateFileParametersDto @params)
    {
        _logger.LogInformation(Util.EventId.CurrentEventId.Value,"Parameters: {0}", @params);
        return await _fileService.CreateFileAsync(@params.ParentDirectoryId, @params.FileName)
            .Map(file => new FileDto(file.FileId, file.ParentDirectoryId, file.FileName))
            .ToActionResult(this, _logger);
    }

    [HttpGet("search")]
    public async Task<ActionResult<FileDto[]>> GetFiles([FromQuery] FileSearchParametersDto @params)
    {
        _logger.LogInformation(Util.EventId.CurrentEventId.Value,"Parameters: {0}", @params);
        return await _fileService.SearchFiles(@params.RootDirectoryId, @params.FileNamePrefix, 10)
            .Map(files => files
                .Select(file => new FileDto(file.FileId, file.ParentDirectoryId, file.FileName))
                .ToArray())
            .ToActionResult(this, _logger);
    }
    
    [HttpDelete("{fileId}")]
    public async Task<ActionResult<long>> DeleteFile(long fileId)
    {
        _logger.LogInformation(Util.EventId.CurrentEventId.Value,"FileId: {0}", fileId);
        return await _fileService.DeleteFileAsync(fileId) 
            .ToActionResult(this, _logger);
    }
}