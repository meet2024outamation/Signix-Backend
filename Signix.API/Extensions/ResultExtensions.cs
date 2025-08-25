using Ardalis.Result;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Signix.API.Extensions;

public static class ResultExtensions
{
    public static ActionResult ToActionResult<T>(this Result<T> result)
    {
        return CreateObjectResult(
            result.Status,
            result.Value,
            result.IsSuccess,
            result.Errors,
            result.ValidationErrors,
            result.CorrelationId
        );
    }

    public static ActionResult ToActionResult<T>(this PagedResult<T> result)
    {
        return CreateObjectResult(
            result.Status,
            result.Value,
            result.IsSuccess,
            result.Errors,
            result.ValidationErrors,
            result.CorrelationId,
            result.PagedInfo
        );
    }

    public static ActionResult ToActionResult(this Result result)
    {
        return CreateObjectResult(
            result.Status,
            result.Value,
            result.IsSuccess,
            result.Errors,
            result.ValidationErrors,
            result.CorrelationId
        );
    }


    private static ObjectResult CreateObjectResult<T>(ResultStatus status,
        T value,
        bool isSuccess,
        IEnumerable<string> errors,
        IEnumerable<ValidationError> validationErrors,
        string correlationId,
        PagedInfo? pagedInfo = null)
    {
        var statusCode = GetStatusCode(status);

        var responseObject = CreateObject(status, value, isSuccess, errors, validationErrors, correlationId, pagedInfo);

        return new ObjectResult(responseObject)
        {
            StatusCode = statusCode
        };
    }

    private static object CreateObject<T>(ResultStatus status,
        T value,
        bool isSuccess,
        IEnumerable<string> errors,
        IEnumerable<ValidationError> validationErrors,
        string correlationId,
        PagedInfo? pagedInfo = null) =>
        new
        {
            status = GetStatusCode(status),
            success = isSuccess,
            value,
            pagedInfo,
            errors,
            validationErrors,
            CorrelationId = correlationId
        };

    public static async Task UpdateHttpContextResponse(this Result result, HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(result.Status);

        await context.Response.WriteAsync(JsonSerializer.Serialize(CreateObject
        (result.Status, result.Value, result.IsSuccess, result.Errors, result.ValidationErrors,
            result.CorrelationId)));
    }


    private static int GetStatusCode(ResultStatus status)
    {
        return status switch
        {
            ResultStatus.Ok => StatusCodes.Status200OK,
            ResultStatus.Created => StatusCodes.Status201Created,
            ResultStatus.NoContent => StatusCodes.Status204NoContent,
            ResultStatus.NotFound => StatusCodes.Status404NotFound,
            ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
            ResultStatus.Invalid => StatusCodes.Status400BadRequest,
            ResultStatus.Error => StatusCodes.Status500InternalServerError,
            ResultStatus.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
