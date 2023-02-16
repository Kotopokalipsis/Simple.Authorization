using System;

namespace Application.Common.Interfaces.Infrastructure.Services
{
    public interface IJwtGenerator
    {
        public DateTime AccessTokenExpires();
        public DateTime RefreshTokenExpires();
        public string CreateAccessToken(Guid id);
        public string CreateRefreshToken(Guid id);
    }
}