using Hospital_Software.Models;

namespace Hospital_Software.Token
{
    public interface ITokenService
    {
        Task<string> GenerateToken(ApplicationUser user);
    }
}
