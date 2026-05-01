using ClientEcommerce.API.DTOs;
using ClientEcommerce.API.DTOs.Admin;

namespace ClientEcommerce.API.Services
{
    public interface IUserService
    {
        void CreateUser(CreateUserDto dto);

        PagedResultDto<AdminUserDto> GetAllUsers(int page, int pageSize);

        UserDetailsDto? GetUserDetails(int userId);

        void ToggleUserStatus(int userId);

        UserProfileDto GetProfile(int userId);

        void UpdateProfile(int userId, UpdateUserProfileDto dto);

        void ChangePassword(int userId, ChangePasswordDto dto);
    }
}
