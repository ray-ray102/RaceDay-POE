namespace RaceDay_POE_API.Services
{
    // Hashes and checks passwords so plain text passwords are never stored
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }
}