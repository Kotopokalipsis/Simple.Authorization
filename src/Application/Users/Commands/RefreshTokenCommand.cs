using System;
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
    public record RefreshTokenCommand : IRequest<IBaseResponse<Token>>
    {
        public string RefreshToken { get; init; }
    }

    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, IBaseResponse<Token>>
    {
        private readonly IJwtReader _jwtReader;
        private readonly IRepository<UserRefreshToken> _refreshTokenRepository;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly UserManager<User> _userManager;

        public RefreshTokenHandler(
            IJwtReader jwtReader,
            UserManager<User> userManager, 
            IRepository<UserRefreshToken> userRefreshTokenRepository,
            IJwtGenerator jwtGenerator,
            IRefreshTokenGenerator refreshTokenGenerator)
        {
            _jwtReader = Guard.Against.Null(jwtReader, nameof(jwtReader));
            _userManager = Guard.Against.Null(userManager, nameof(userManager));
            _refreshTokenRepository = Guard.Against.Null(userRefreshTokenRepository, nameof(userRefreshTokenRepository));
            _jwtGenerator = Guard.Against.Null(jwtGenerator, nameof(jwtGenerator));
            _refreshTokenGenerator = Guard.Against.Null(refreshTokenGenerator, nameof(refreshTokenGenerator));;
        }

        public async Task<IBaseResponse<Token>> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            var claims = _jwtReader.GetClaimFromToken(request.RefreshToken);

            var user = 
                await _userManager.FindByIdAsync(
                    claims
                        .Where(x => x.Type == "jti")
                        .Select(x => x.Value)
                        .FirstOrDefault()
                );

            if (user == null)
                return GetValidationErrorResponse();

            var refreshTokenString = await _refreshTokenGenerator.Generate(user);

            var userRefreshToken = await _refreshTokenRepository.FindOneBy(x => x.UserId == user.Id);
            userRefreshToken.RefreshToken = refreshTokenString;

            if (!userRefreshToken.IsTransient())
                return new BaseResponse<Token>
                {
                    StatusCode = 200,
                    Data = new Token
                        {AccessToken = _jwtGenerator.CreateAccessToken(user.Id), RefreshToken = refreshTokenString}
                };

            return new RollbackTransactionErrorResponse<Token>
            {
                StatusCode = 400,
                Errors = new Dictionary<string, List<string>>
                    {{"LoginError", new List<string>() {"Error occurred while trying to create refresh token."}}},
            };
        }

        private static CommitTransactionErrorResponse<Token> GetValidationErrorResponse() => new()
        {
            StatusCode = 400,
            Errors = new Dictionary<string, List<string>>{{"ValidationError", new List<string> {"Refresh token not valid"}}},
        };
    }
}