using Microsoft.AspNetCore.Http;

namespace Domain.Exceptions
{
    public enum ErrorCode
    {
        BadRequest = StatusCodes.Status400BadRequest,
        Unauthorized = StatusCodes.Status401Unauthorized,
        Forbidden = StatusCodes.Status403Forbidden,
        NotFound = StatusCodes.Status404NotFound,
        Conflict = StatusCodes.Status409Conflict,
        InternalServerError = StatusCodes.Status500InternalServerError,
    }
}
