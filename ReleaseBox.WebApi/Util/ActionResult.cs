using Common.Error;
using Common.Expect;
using Microsoft.AspNetCore.Mvc;
using ReleaseBox.Core.Data.ErrorCodes;

namespace ReleaseBox.Util;

public static class ActionResult
{
    public static async Task<ActionResult<TOk>> ToActionResult<TOk, TController>(this Task<Result<TOk, Error<CreateErrorCodes>>> asyncResult, TController controller, ILogger<TController> logger)
        where TController : ControllerBase
    {
        return await asyncResult.ToActionResult(controller, logger, ToErrorActionResult);
        
        ActionResult<TOk> ToErrorActionResult(Error<CreateErrorCodes> error) =>
            error.Code switch
            {
                CreateErrorCodes.Invalid => controller.BadRequest(error),
                CreateErrorCodes.Duplicate => controller.Conflict(error),
                CreateErrorCodes.ParentNotFound => controller.NotFound(error),
                CreateErrorCodes.UnknownError => controller.UnprocessableEntity(error),
                _ => throw new Exception($"Unknown error code: {error.Code}. Dumping error: {error}")
            };
    }
    
    public static async Task<ActionResult<TOk>> ToActionResult<TOk, TController>(this Task<Result<TOk, Error<GetErrorCodes>>> asyncResult, TController controller, ILogger<TController> logger)
        where TController : ControllerBase
    {
        return await asyncResult.ToActionResult(controller, logger, ToErrorActionResult);
        
        ActionResult<TOk> ToErrorActionResult(Error<GetErrorCodes> error) =>
            error.Code switch
            {
                GetErrorCodes.EntityNotFound => controller.NotFound(error),
                GetErrorCodes.UnknownError => controller.UnprocessableEntity(error),
                _ => throw new Exception($"Unknown error code: {error.Code}. Dumping error: {error}")
            };
    }
    
    public static async Task<ActionResult<TOk>> ToActionResult<TOk, TController>(this Task<Result<TOk, Error<DeleteErrorCodes>>> asyncResult, TController controller, ILogger<TController> logger)
        where TController : ControllerBase
    {
        return await asyncResult.ToActionResult(controller, logger, ToErrorActionResult);
        
        ActionResult<TOk> ToErrorActionResult(Error<DeleteErrorCodes> error) =>
            error.Code switch
            {
                DeleteErrorCodes.EntityNotFound => controller.NotFound(error),
                DeleteErrorCodes.UnknownError => controller.UnprocessableEntity(error),
                _ => throw new Exception($"Unknown error code: {error.Code}. Dumping error: {error}")
            };
    }

    private static async Task<ActionResult<TOk>> ToActionResult<TOk, TErrorCode, TController>(this Task<Result<TOk, Error<TErrorCode>>> asyncResult, TController controller, ILogger<TController> logger,
            Func<Error<TErrorCode>, ActionResult<TOk>> errorMapper)
        where TController : ControllerBase
        where TErrorCode : struct, Enum 
    {
        return await asyncResult.Match(SuccessMapper, ErrorMapper);

        ActionResult<TOk> SuccessMapper(TOk ok)
        {
            const string successLogTemplate = "Success: {0}"; 
            logger.LogInformation(EventId.CurrentEventId.Value, successLogTemplate, ok);
            return controller.Ok(ok);
        }

        ActionResult<TOk> ErrorMapper(Error<TErrorCode> error)
        {
            const string errorLogTemplate = "Error: {0} -> {1}";
            if (error.Exception.IsSome(out var exc))
            {
                logger.LogError(EventId.CurrentEventId.Value, exc, errorLogTemplate, error.Code, error.Description);
            }
            else
            {
                logger.LogError(EventId.CurrentEventId.Value, errorLogTemplate, error.Code, error.Description);
            }
            return errorMapper(error);
        }                                                   
    }
    
}