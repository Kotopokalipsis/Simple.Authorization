using System.Collections.Generic;
using System.Linq;
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
    public record RegistrationCommand : IRequest<IBaseResponse<Token>>
    {
        public string UserName { get; init; }
        public string Email { get; init; }
        public string Password { get; init; }
    }
    
    public class RegistrationHandler : IRequestHandler<RegistrationCommand, IBaseResponse<Token>>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRepository<UserRefreshToken> _refreshTokenRepository;

        public RegistrationHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtGenerator jwtGenerator,
            IRefreshTokenGenerator refreshTokenGenerator,
            IRepository<UserRefreshToken> refreshTokenRepository)
        {
            _userManager = Guard.Against.Null(userManager, nameof(userManager));
            _signInManager = Guard.Against.Null(signInManager, nameof(signInManager));
            _jwtGenerator = Guard.Against.Null(jwtGenerator, nameof(jwtGenerator));
            _refreshTokenGenerator = Guard.Against.Null(refreshTokenGenerator, nameof(refreshTokenGenerator));
            _refreshTokenRepository = Guard.Against.Null(refreshTokenRepository, nameof(refreshTokenRepository));
        }

        public async Task<IBaseResponse<Token>> Handle(RegistrationCommand request, CancellationToken ct)
        {
            var user = new User
            {
                Email = request.Email,
                UserName = request.UserName
            };
                
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new RollbackTransactionErrorResponse<Token>
                {
                    StatusCode = 400,
                    Errors = new Dictionary<string, List<string>>{{"CreateError", result.Errors.Select(x => x.Description.ToString()).ToList()}},
                };
            }

            await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            var refreshTokenString = await _refreshTokenGenerator.Generate(user);
            
            var userRefreshToken = _refreshTokenRepository.Add(new UserRefreshToken {RefreshToken = refreshTokenString, User = user});

            if (userRefreshToken.IsTransient())
            {
                return new RollbackTransactionErrorResponse<Token>
                {
                    StatusCode = 400,
                    Errors = new Dictionary<string, List<string>>{{"CreateError", new List<string> {"Error occurred while trying to create new user"}}},
                };
            }
            
            return new BaseResponse<Token>
            {
                StatusCode = 201,
                Data = new Token {AccessToken = _jwtGenerator.CreateAccessToken(user.Id), RefreshToken = refreshTokenString}
            };
        }
    }
}