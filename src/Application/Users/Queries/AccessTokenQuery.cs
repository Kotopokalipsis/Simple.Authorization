using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Application.Common.Interfaces.Application.Services;
using Application.Common.Interfaces.Infrastructure.Services;
using Application.Common.Interfaces.Infrastructure.UnitOfWork;
using Application.Common.Responses;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Queries;

public record AccessTokenQuery : IRequest<IBaseResponse<Token>>
{
    
}

public class AccessTokenHandler : IRequestHandler<AccessTokenQuery, IBaseResponse<Token>>
{
    private readonly IJwtReader _jwtReader;
    private readonly UserManager<User> _userManager;
    private readonly ITokenHelper _tokenHelper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICookieHelper _cookieHelper;

    public AccessTokenHandler(
        IJwtReader jwtReader, 
        UserManager<User> userManager,
        ITokenHelper tokenHelper,
        IUnitOfWork unitOfWork,
        ICookieHelper cookieHelper)
    {
        _tokenHelper = Guard.Against.Null(tokenHelper, nameof(tokenHelper));
        _unitOfWork = Guard.Against.Null(unitOfWork, nameof(unitOfWork));
        _cookieHelper = Guard.Against.Null(cookieHelper, nameof(cookieHelper));
        _jwtReader = Guard.Against.Null(jwtReader, nameof(jwtReader));
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
    }
    
    public async Task<IBaseResponse<Token>> Handle(AccessTokenQuery request, CancellationToken cancellationToken)
    {
        var refreshToken = _cookieHelper.GetRefreshTokenFromCookie();

        var user = await _tokenHelper.GetUserByRefreshToken(refreshToken);

        if (user == null)
        {
            _unitOfWork.RefreshTokenBlacklistRepository.Add(new RefreshTokenBlacklist { RefreshToken = refreshToken });
            await _unitOfWork.Commit(cancellationToken);

            return GetValidationErrorResponse();
        }

        _tokenHelper.SetAccessToken(user);
        await _unitOfWork.Commit(cancellationToken);

        return new BaseResponse<Token>
        {
            StatusCode = 200,
            Data = new Token {AccessToken = user.AccessToken}
        };
    }
    
    private static ErrorResponse<Token> GetValidationErrorResponse() => new()
    {
        StatusCode = 400,
        Errors = new Dictionary<string, List<string>>{{"ValidationError", new List<string> {"Refresh token not valid"}}},
    };
}