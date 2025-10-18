using WebApi.Dao;
using WebApi.Dto;

namespace WebApi.Interface
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> CreateUserAsync(CreateUserRequest request);
        Task<User> UpdateUserAsync(UserUpdateRequest request, int userId);
        Task DeleteUserAsync(int userId);
        Task ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<User> GetAuthenticatedUser();
        UserDto ConvertUserToDto(User user);
    }
}
