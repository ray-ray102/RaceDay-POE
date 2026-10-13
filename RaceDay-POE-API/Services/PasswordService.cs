using Microsoft.AspNetCore.Identity;
using RaceDay_POE_API.Models;

namespace RaceDay_POE_API.Services
{
    // Uses the built in ASP.NET Core PasswordHasher, which salts and hashes with PBKDF2
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public string HashPassword(string password)
        {
            return _hasher.HashPassword(new User(), password);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            var result = _hasher.VerifyHashedPassword(new User(), hashedPassword, password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}