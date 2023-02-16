using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Application.Common.Interfaces.Application.Services;
using Application.Common.Interfaces.Infrastructure.Repositories;
using Application.Common.Interfaces.Infrastructure.Services;
using Application.Common.Interfaces.Infrastructure.UnitOfWork;
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
        private readonly ITokenHelper _tokenHelper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICookieHelper _cookieHelper;

        public RegistrationHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager, ITokenHelper tokenHelper, IUnitOfWork unitOfWork, ICookieHelper cookieHelper)
        {
            _tokenHelper = Guard.Against.Null(tokenHelper, nameof(tokenHelper));
            _unitOfWork = Guard.Against.Null(unitOfWork, nameof(unitOfWork));
            _cookieHelper = Guard.Against.Null(cookieHelper, nameof(cookieHelper));
            _userManager = Guard.Against.Null(userManager, nameof(userManager));
            _signInManager = Guard.Against.Null(signInManager, nameof(signInManager));
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
                return new ErrorResponse<Token>
                {
                    StatusCode = 400,
                    Errors = new Dictionary<string, List<string>>{{"CreateError", result.Errors.Select(x => x.Description.ToString()).ToList()}},
                };
            }

            await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            await _tokenHelper.SetRefreshToken(user);

            user.CreationDate = DateTime.Now.ToUniversalTime();
            user.EmailConfirmed = true;
            
            await _unitOfWork.Commit(ct);
            
            _cookieHelper.SetRefreshTokenInCookie(user.RefreshToken);

            return new BaseResponse<Token>
            {
                StatusCode = 201,
                Data = null
            };
        }
    }
}