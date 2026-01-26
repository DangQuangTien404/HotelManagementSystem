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
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository repository, IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _repository.GetAllWithDetailsAsync();
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
            return user != null ? MapToDto(user) : null;
        }

        public async Task AddUserAsync(UserDto userDto)
        {
            var user = MapToEntity(userDto);
            user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);
            user.CreatedAt = DateTime.UtcNow;

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
                user.FullName = userDto.FullName;
                user.Email = userDto.Email;
                user.Role = userDto.Role;
                
                // Only update password if provided
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(userDto.Password);
                }

                await _repository.UpdateAsync(user);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsStaff = user.Role == "Staff"
            };
        }

        private static User MapToEntity(UserDto dto)
        {
            return new User
            {
                Id = dto.Id,
                Username = dto.Username,
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role
            };
        }
    }
}
