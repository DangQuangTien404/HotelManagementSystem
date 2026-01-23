using BLL.Interfaces;
using DAL.Interfaces;
using DTOs.Entities;
using DTOs.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            if (await _userRepository.UsernameExistsAsync(model.Username))
            {
                return (false, "Username already exists");
            }

            if (await _userRepository.EmailExistsAsync(model.Email))
            {
                return (false, "Email already exists");
            }

            string passwordHash = HashPassword(model.Password);

            var user = new User
            {
                Username = model.Username,
                PasswordHash = passwordHash,
                FullName = model.FullName,
                Email = model.Email,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddUserAsync(user);
            await _userRepository.SaveChangesAsync();

            return (true, "Registration successful");
        }

        public async Task<(bool Success, User? User, string Message)> LoginAsync(LoginViewModel model)
        {
            var user = await _userRepository.GetByUsernameAsync(model.Username);

            if (user == null)
            {
                return (false, null, "Invalid username or password");
            }

            if (!VerifyPassword(model.Password, user.PasswordHash))
            {
                return (false, null, "Invalid username or password");
            }

            return (true, user, "Login successful");
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                string computedHash = Convert.ToBase64String(hashBytes);

                return computedHash == hash;
            }
        }
    }
}
