using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApi.Common.Exceptions;
using WebApi.Dao;
using WebApi.Data;
using WebApi.Dto;
using WebApi.Interface;

namespace WebApi.Service
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID");
                }

                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new ResourceNotFoundException($"User with id {userId} not found");
                }

                _logger.LogInformation($"User fetched successfully for ID: {userId}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error fetching user by ID {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<User> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException("User request cannot be null");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    throw new ArgumentException("Email cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new ArgumentException("Password cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                {
                    throw new ArgumentException("First name and last name cannot be empty");
                }

                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUser != null)
                {
                    throw new UserAlreadyExistException($"User with email '{request.Email}' already exists");
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Password = HashPassword(request.Password)
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User created successfully with email: {user.Email}");
                return user;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while creating user: {ex.Message}");
                throw new InvalidOperationException("Error creating user", ex);
            }
        }

        public async Task<User> UpdateUserAsync(UserUpdateRequest request, int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID");
                }

                if (request == null)
                {
                    throw new ArgumentException("User request cannot be null");
                }

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                {
                    throw new ArgumentException("First name and last name cannot be empty");
                }

                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    throw new ResourceNotFoundException($"User with id {userId} not found");
                }

                // Check if new email is already taken by another user
                if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
                {
                    var emailExists = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != userId);

                    if (emailExists != null)
                    {
                        throw new UserAlreadyExistException($"User with email '{request.Email}' already exists");
                    }
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.Email = request.Email;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User with ID {userId} updated successfully");
                return user;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while updating user: {ex.Message}");
                throw new InvalidOperationException("Error updating user", ex);
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID");
                }

                var user = await _context.Users
                    .Include(u => u.Carts)
                    .Include(u => u.Orders)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new ResourceNotFoundException($"User with id {userId} not found");
                }

                // Delete related data (cascade will handle some, but we can log warnings)
                if (user.Carts?.Count > 0)
                {
                    _logger.LogWarning($"Deleting {user.Carts.Count} carts for user {userId}");
                }

                if (user.Orders?.Count > 0)
                {
                    _logger.LogWarning($"Deleting {user.Orders.Count} orders for user {userId}");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User with ID {userId} deleted successfully");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while deleting user: {ex.Message}");
                throw new InvalidOperationException("Error deleting user", ex);
            }
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentException("Reset password request cannot be null");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    throw new ArgumentException("Email cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(request.OldPassword))
                {
                    throw new ArgumentException("Old password cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    throw new ArgumentException("New password cannot be empty");
                }

                if (request.NewPassword.Length < 6)
                {
                    throw new ArgumentException("New password must be at least 6 characters long");
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    throw new ResourceNotFoundException($"User with email '{request.Email}' not found");
                }

                // Verify old password
                if (!VerifyPassword(request.OldPassword, user.Password))
                {
                    _logger.LogWarning($"Invalid old password attempt for user {user.Id}");
                    throw new InvalidPasswordException("Old password is incorrect");
                }

                // Update password
                user.Password = HashPassword(request.NewPassword);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password reset successfully for user: {user.Email}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error while resetting password: {ex.Message}");
                throw new InvalidOperationException("Error resetting password", ex);
            }
        }

        public async Task<User> GetAuthenticatedUser()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new JwtAuthenticationException("User not authenticated");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new JwtAuthenticationException("User not found");
            }

            return user;
        }

        public UserDto ConvertUserToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        // Password hashing and verification methods
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}