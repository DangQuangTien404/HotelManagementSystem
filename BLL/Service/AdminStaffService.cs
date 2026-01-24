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
    public class AdminStaffService : IAdminStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AdminStaffService(
            IStaffRepository staffRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<StaffDto>> GetAllStaffAsync()
        {
            var staffs = await _staffRepository.GetAllStaffWithUserAsync();
            return staffs.Select(MapToDto);
        }

        public async Task<StaffDto?> GetStaffByIdAsync(int id)
        {
            var staff = await _staffRepository.GetStaffWithUserByIdAsync(id);
            return staff == null ? null : MapToDto(staff);
        }

        public async Task AddStaffAsync(StaffDto staffDto)
        {
            // Check if username already exists
            var existingUser = await _userRepository.GetAllAsync();
            if (existingUser.Any(u => u.Username == staffDto.Username))
            {
                throw new Exception("Username already exists.");
            }

            // Create User
            var user = new User
            {
                Username = staffDto.Username,
                PasswordHash = _passwordHasher.HashPassword(staffDto.Password),
                Role = "Staff",
                FullName = staffDto.FullName,
                Email = staffDto.Email,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Create Staff
            var staff = new Staff
            {
                UserId = user.Id,
                Position = staffDto.Position,
                Shift = staffDto.Shift,
                HireDate = staffDto.HireDate
            };

            await _staffRepository.AddAsync(staff);
        }

        public async Task UpdateStaffAsync(StaffDto staffDto)
        {
            var staff = await _staffRepository.GetStaffWithUserByIdAsync(staffDto.Id);
            if (staff == null)
            {
                throw new Exception("Staff not found.");
            }

            // Update User
            var user = staff.User;
            user.FullName = staffDto.FullName;
            user.Email = staffDto.Email;
            
            // Update password if provided
            if (!string.IsNullOrEmpty(staffDto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(staffDto.Password);
            }

            await _userRepository.UpdateAsync(user);

            // Update Staff
            staff.Position = staffDto.Position;
            staff.Shift = staffDto.Shift;
            staff.HireDate = staffDto.HireDate;

            await _staffRepository.UpdateAsync(staff);
        }

        public async Task DeleteStaffAsync(int id)
        {
            var staff = await _staffRepository.GetStaffWithUserByIdAsync(id);
            if (staff != null)
            {
                // Delete user first, Staff will be deleted by cascade
                await _userRepository.DeleteAsync(staff.UserId);
            }
        }

        private StaffDto MapToDto(Staff staff)
        {
            return new StaffDto
            {
                Id = staff.Id,
                UserId = staff.UserId,
                Username = staff.User?.Username ?? string.Empty,
                FullName = staff.User?.FullName ?? string.Empty,
                Email = staff.User?.Email ?? string.Empty,
                Position = staff.Position,
                Shift = staff.Shift,
                HireDate = staff.HireDate
            };
        }
    }
}

