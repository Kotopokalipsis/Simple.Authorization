using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Services;
using Application.Common.Interfaces.Infrastructure.Repositories;
using Application.Common.Interfaces.Infrastructure.Services;
using Domain.Entities;

namespace Application.Common.Services.RefreshTokenGenerator
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IRepository<RefreshTokenBlacklist> _refreshTokenBlacklistRepository;

        public RefreshTokenGenerator(IJwtGenerator jwtGenerator, IRepository<RefreshTokenBlacklist> refreshTokenBlacklistRepository)
        {
            _jwtGenerator = jwtGenerator;
            _refreshTokenBlacklistRepository = refreshTokenBlacklistRepository;
        }

        public async Task<string> Generate(User user)
        {
            string refreshTokenString;
            
            while (true)
            {
                refreshTokenString = _jwtGenerator.CreateRefreshToken(user.Id);
                var refreshTokenBlacklist = await _refreshTokenBlacklistRepository.FindOneBy(x => x.RefreshToken == refreshTokenString);
                
                if (refreshTokenBlacklist == null)
                    break;
            }

            _refreshTokenBlacklistRepository.Add(new RefreshTokenBlacklist {RefreshToken = refreshTokenString});

            return refreshTokenString;
        }
    }
}