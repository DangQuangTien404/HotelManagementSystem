using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Entities;
using DTOs.ViewModels;

namespace BLL.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, User? User, string Message)> LoginAsync(LoginViewModel model);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
