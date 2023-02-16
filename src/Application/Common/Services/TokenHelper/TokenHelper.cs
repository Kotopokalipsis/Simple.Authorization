using System;
using System.Linq;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Services;
using Application.Common.Interfaces.Infrastructure.Services;
using Application.Common.Interfaces.Infrastructure.UnitOfWork;
using Domain.Entities;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;

namespace Application.Common.Services.TokenHelper;

public class TokenHelper : ITokenHelper
{
    private readonly IJwtGenerator _jwtGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtReader _jwtReader;
    private readonly UserManager<User> _userManager;

    public TokenHelper(IJwtGenerator jwtGenerator, IUnitOfWork unitOfWork, IJwtReader jwtReader, UserManager<User> userManager)
    {
        _jwtGenerator = jwtGenerator;
        _unitOfWork = unitOfWork;
        _jwtReader = jwtReader;
        _userManager = userManager;
    }

    public void SetAccessToken(User user)
    {
        user.AccessToken = _jwtGenerator.CreateAccessToken(user.Id);
    }
    
    public async Task SetRefreshToken(User user)
    {
        string refreshTokenString;
            
        while (true)
        {
            refreshTokenString = _jwtGenerator.CreateRefreshToken(user.Id);
            var refreshTokenBlacklist = await _unitOfWork.RefreshTokenBlacklistRepository.FindOneBy(x => x.RefreshToken == refreshTokenString);
                
            if (refreshTokenBlacklist == null)
                break;
        }
        
        user.RefreshToken = refreshTokenString;
        user.RefreshTokenExpirationTime = _jwtGenerator.RefreshTokenExpires();
    }

    [ItemCanBeNull]
    public async Task<User> GetUserByRefreshToken(string refreshToken)
    {
        var blacklist = 
            await _unitOfWork.RefreshTokenBlacklistRepository
                .FindOneBy(x => x.RefreshToken == refreshToken);

        if (blacklist != null) return null;

        var claims = _jwtReader.GetClaimFromToken(refreshToken);

        var user = 
            await _userManager.FindByIdAsync(
                claims
                    .Where(x => x.Type == "jti")
                    .Select(x => x.Value)
                    .FirstOrDefault()
            );

        if (user == null) return null;

        return DateTime.Now < user.RefreshTokenExpirationTime ? user : null;
    }
}