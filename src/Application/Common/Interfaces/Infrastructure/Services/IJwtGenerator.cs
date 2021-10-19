using System;

namespace Application.Common.Interfaces.Infrastructure.Services
{
    public interface IJwtGenerator
    {
        string CreateAccessToken(Guid id);
        string CreateRefreshToken(Guid id);
    }
}