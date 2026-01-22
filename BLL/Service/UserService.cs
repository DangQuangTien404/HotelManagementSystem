using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        // Ideally we would inject IStaffRepository too if we needed to create Staff records explicitly,
        // but for now we'll assume setting the role or existence makes them relevant.
        // Actually, the requirement says "manage users".

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _repository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<IEnumerable<UserDto>> GetStaffUsersAsync()
        {
            var users = await _repository.GetStaffUsersAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        public async Task AddUserAsync(UserDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                PasswordHash = userDto.Password, // In real app, hash this!
                Role = userDto.Role,
                FullName = userDto.FullName,
                Email = userDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            // If Role is Staff, automatically create a Staff entry
            if (string.Equals(userDto.Role, "Staff", StringComparison.OrdinalIgnoreCase))
            {
                var staff = new Staff
                {
                    Position = "General Staff",
                    Shift = "Day",
                    HireDate = DateTime.Now,
                    User = user
                };
                user.Staffs.Add(staff);
            }

            await _repository.AddAsync(user);
        }

        public async Task UpdateUserAsync(UserDto userDto)
        {
            var user = await _repository.GetByIdAsync(userDto.Id);
            if (user != null)
            {
                user.Username = userDto.Username;
                user.Role = userDto.Role;
                user.FullName = userDto.FullName;
                user.Email = userDto.Email;
                // Only update password if provided
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.PasswordHash = userDto.Password;
                }

                await _repository.UpdateAsync(user);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                // Check if they are in staff table?
                // For now, IsStaff is loosely based on role or just false if we don't load navigation prop.
                // The GetStaffUsersAsync ensures we get the right ones.
                IsStaff = user.Staffs != null && user.Staffs.Any()
            };
        }
    }
}
