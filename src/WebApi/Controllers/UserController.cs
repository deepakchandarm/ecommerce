using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Common.Dto;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a specific user by ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve</param>
        /// <returns>ResponseEntity containing ApiResponse with user details</returns>
        [HttpGet("{userId}/user")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                var userDto = _userService.ConvertUserToDto(user);
                _logger.LogInformation($"User fetched successfully for ID: {userId}");
                return Ok(new ApiResponse<UserDto>("User fetched successfully", userDto));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogError($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid user ID: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error fetching user: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error fetching user", ex.Message));
            }
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="request">CreateUserRequest object with user details</param>
        /// <returns>ResponseEntity containing ApiResponse with the created user</returns>
        [HttpPost("add")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>("User request cannot be null", null));
                }

                var user = await _userService.CreateUserAsync(request);
                var userDto = _userService.ConvertUserToDto(user);
                return Ok(new ApiResponse<UserDto>("User created successfully", userDto));
            }
            catch (UserAlreadyExistException ex)
            {
                _logger.LogWarning($"User already exists: {ex.Message}");
                return StatusCode(StatusCodes.Status409Conflict,
                    new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid user data: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating user: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error creating user", ex.Message));
            }
        }

        /// <summary>
        /// Update an existing user.
        /// </summary>
        /// <param name="request">UserUpdateRequest object with updated user details</param>
        /// <param name="userId">The ID of the user to update</param>
        /// <returns>ResponseEntity containing ApiResponse with the updated user</returns>
        [HttpPut("{userId}/update")]
        [Authorize]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest request, int userId)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new ApiResponse<string>("User request cannot be null", null));
                }

                var user = await _userService.UpdateUserAsync(request, userId);
                var userDto = _userService.ConvertUserToDto(user);
                return Ok(new ApiResponse<UserDto>("User updated successfully", userDto));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (UserAlreadyExistException ex)
            {
                _logger.LogWarning($"Email already in use: {ex.Message}");
                return StatusCode(StatusCodes.Status409Conflict,
                    new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid user data: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error updating user", ex.Message));
            }
        }

        /// <summary>
        /// Delete a user by ID.
        /// </summary>
        /// <param name="userId">The ID of the user to delete</param>
        /// <returns>ResponseEntity containing ApiResponse with success or error message</returns>
        [HttpDelete("{userId}/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                await _userService.DeleteUserAsync(userId);
                return Ok(new ApiResponse<string>("User deleted successfully", null));
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid user ID: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error deleting user", ex.Message));
            }
        }

        /// <summary>
        /// Reset a user's password.
        /// </summary>
        /// <param name="resetPasswordRequestDto">ResetPasswordRequestDto object with email and password details</param>
        /// <returns>ResponseEntity with success or error message</returns>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto resetPasswordRequestDto)
        {
            try
            {
                if (resetPasswordRequestDto == null)
                {
                    return BadRequest(new ApiResponse<string>("Reset password request cannot be null", null));
                }

                await _userService.ResetPasswordAsync(resetPasswordRequestDto);
                return Ok("Password reset successfully");
            }
            catch (ResourceNotFoundException ex)
            {
                _logger.LogWarning($"User not found: {ex.Message}");
                return NotFound(new ApiResponse<string>(ex.Message, null));
            }
            catch (InvalidPasswordException ex)
            {
                _logger.LogWarning($"Invalid password: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid reset password data: {ex.Message}");
                return BadRequest(new ApiResponse<string>(ex.Message, null));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error resetting password: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>("Error resetting password", ex.Message));
            }
        }
    }
}
