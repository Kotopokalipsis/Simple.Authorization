using Microsoft.IdentityModel.Tokens;

namespace Application.Common.Interfaces.Infrastructure.Services
{
    public interface IJwtValidator
    {
        bool Validate(string token, SymmetricSecurityKey key);
    }
}