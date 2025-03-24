using Common.Expect;
using Microsoft.AspNetCore.Mvc;
using ReleaseBox.Core.Interfaces;
using ReleaseBox.Models;
using ReleaseBox.Util;

namespace ReleaseBox.Controllers;

[ApiController]
[Route("api/directory")]
public class DirectoryController : ControllerBase
{
    private readonly IDirectoryService _directoryService;
    private readonly ILogger<DirectoryController> _logger;

    public DirectoryController(IDirectoryService directoryService, ILogger<DirectoryController> logger)
    {
        _directoryService = directoryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<DirectoryDto>> CreateDirectory([FromBody] CreateDirectoryParametersDto @params)
    {
        _logger.LogInformation(Util.EventId.CurrentEventId.Value,"Parameters: {0}", @params);
        return await _directoryService.CreateDirectoryAsync(@params.ParentDirectoryId, @params.DirectoryName)
            .Map(directory => new DirectoryDto(directory.DirectoryId, directory.ParentDirectoryId, directory.DirectoryName))
            .ToActionResult(this, _logger);
    }
    
    [HttpDelete("{directoryId}")]
    public async Task<ActionResult<long>> DeleteFile(long directoryId)
    {
        _logger.LogInformation(Util.EventId.CurrentEventId.Value,"DirectoryId: {0}", directoryId);
        return await _directoryService.DeleteDirectoryAsync(directoryId) 
            .ToActionResult(this, _logger);
    }

}