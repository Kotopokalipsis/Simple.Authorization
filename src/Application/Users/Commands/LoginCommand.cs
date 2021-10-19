using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Application.Common.Interfaces.Application.Services;
using Application.Common.Interfaces.Infrastructure.Repositories;
using Application.Common.Interfaces.Infrastructure.Services;
using Application.Common.Responses;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands
{
    public record LoginCommand : IRequest<IBaseResponse<Token>>
    {
        public string Email { get; init; }
        public string Password { get; init; }
    }
    
    public class LoginHandler : IRequestHandler<LoginCommand, IBaseResponse<Token>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IRepository<UserRefreshToken> _refreshTokenRepository;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;

        public LoginHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator,
            IRepository<UserRefreshToken> refreshTokenRepository,
            IRefreshTokenGenerator refreshTokenGenerator)
        {
            _userManager = Guard.Against.Null(userManager, nameof(userManager));
            _signInManager = Guard.Against.Null(signInManager, nameof(signInManager));
            _jwtGenerator = Guard.Against.Null(jwtGenerator, nameof(jwtGenerator));
            _refreshTokenRepository = Guard.Against.Null(refreshTokenRepository, nameof(refreshTokenRepository));
            _refreshTokenGenerator = Guard.Against.Null(refreshTokenGenerator, nameof(refreshTokenGenerator));
        }

        public async Task<IBaseResponse<Token>> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user.IsTransient())
            {
                return new RollbackTransactionErrorResponse<Token>
                {
                    StatusCode = 401,
                    Errors = new Dictionary<string, List<string>>{{"LoginError", new List<string> {"Access denied"}}},
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                var refreshTokenString = await _refreshTokenGenerator.Generate(user);
                
                var userRefreshToken = await _refreshTokenRepository.FindOneBy(x => x.UserId == user.Id);
                userRefreshToken.RefreshToken = refreshTokenString;
                
                if (!userRefreshToken.IsTransient())
                    return new BaseResponse<Token>
                    {
                        StatusCode = 200,
                        Data = new Token {AccessToken = _jwtGenerator.CreateAccessToken(user.Id), RefreshToken = refreshTokenString}
                    };
                
                return new RollbackTransactionErrorResponse<Token>
                {
                    StatusCode = 400,
                    Errors = new Dictionary<string, List<string>>{{"LoginError", new List<string>() {"Error occurred while trying to create refresh token"}}},
                };
            }

            return new RollbackTransactionErrorResponse<Token>
            {
                StatusCode = 401,
                Errors = new Dictionary<string, List<string>>{{"LoginError", new List<string> {"Access denied"}}},
            };
        }
    }
}