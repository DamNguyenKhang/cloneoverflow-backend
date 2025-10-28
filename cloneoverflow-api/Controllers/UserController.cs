using System.Security.Claims;
using Application.DTOs.Responses;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace cloneoverflow_api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController(IUserService userService) : ControllerBase
    {
        [HttpGet("my-info")]
        public async Task<ActionResult<ApiResponse<UserResponse>>> GetMyInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userService.GetByIdAsync(userId!);


            return Ok(new ApiResponse<UserResponse>
            {
                Message = "User info retrieved successfully",
                Result = user
            });
        }
    }
}
