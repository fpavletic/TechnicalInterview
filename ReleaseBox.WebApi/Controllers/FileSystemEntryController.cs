using Common.Expect;
using Microsoft.AspNetCore.Mvc;
using ReleaseBox.Core.Interfaces;
using ReleaseBox.Models;
using ReleaseBox.Util;

namespace ReleaseBox.Controllers;


[ApiController]
[Route("api/filesystementry")]
public class FileSystemEntryController : ControllerBase
{
    private readonly IFileSystemEntryService _fileSystemEntryService;
    private readonly ILogger<FileSystemEntryController> _logger;

    public FileSystemEntryController(IFileSystemEntryService fileSystemEntryService, ILogger<FileSystemEntryController> logger)
    {
        _fileSystemEntryService = fileSystemEntryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<FileSystemEntryDto[]>> GetFileSystemEntries([FromQuery] long parentDirectoryId)
    {
        _logger.LogInformation(Util.EventId.CurrentEventId.Value, "ParentDirectoryId: {0}", parentDirectoryId);
        return await _fileSystemEntryService.GetFileSystemEntries(parentDirectoryId)
            .Map(fses => fses
                .Select(fse => new FileSystemEntryDto(fse.FileSystemEntryId, fse.ParentDirectoryId, fse.FileSystemEntryName, fse.IsDirectory))
                .ToArray())
            .ToActionResult(this, _logger);
    } 
}