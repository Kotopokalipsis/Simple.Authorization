using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Application.Common.Interfaces.Application.Services;
using Application.Common.Interfaces.Infrastructure.UnitOfWork;
using Application.Common.Responses;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;
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
        private readonly ICookieHelper _cookieHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenHelper _tokenHelper;

        public LoginHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUnitOfWork unitOfWork, ITokenHelper tokenHelper, ICookieHelper cookieHelper)
        {
            _unitOfWork = Guard.Against.Null(unitOfWork, nameof(unitOfWork));
            _tokenHelper = Guard.Against.Null(tokenHelper, nameof(tokenHelper));
            _cookieHelper = Guard.Against.Null(cookieHelper, nameof(cookieHelper));
            _userManager = Guard.Against.Null(userManager, nameof(userManager));
            _signInManager = Guard.Against.Null(signInManager, nameof(signInManager));
        }

        public async Task<IBaseResponse<Token>> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user.IsTransient())
            {
                return new ErrorResponse<Token>
                {
                    StatusCode = 401,
                    Errors = new Dictionary<string, List<string>>{{"LoginError", new List<string> {"Access denied"}}},
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                await _tokenHelper.SetRefreshToken(user);
                user.LastLoginDate = DateTime.Now.ToUniversalTime();

                await _unitOfWork.Commit(ct);

                _cookieHelper.SetRefreshTokenInCookie(user.RefreshToken);

                return new BaseResponse<Token>
                {
                    StatusCode = 200,
                };
            }

            return new ErrorResponse<Token>
            {
                StatusCode = 401,
                Errors = new Dictionary<string, List<string>>{{"LoginError", new List<string> {"Access denied"}}},
            };
        }
    }
}