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

namespace Application.Users.Queries;

public record NewRefreshTokenQuery : IRequest<IBaseResponse<Token>>;

public class NewRefreshTokenHandler : IRequestHandler<NewRefreshTokenQuery, IBaseResponse<Token>>
{
    private readonly ITokenHelper _tokenHelper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICookieHelper _cookieHelper;

    public NewRefreshTokenHandler(ITokenHelper tokenHelper, IUnitOfWork unitOfWork, ICookieHelper cookieHelper)
    {
        _tokenHelper = Guard.Against.Null(tokenHelper, nameof(tokenHelper));
        _unitOfWork = Guard.Against.Null(unitOfWork, nameof(unitOfWork));
        _cookieHelper = Guard.Against.Null(cookieHelper, nameof(cookieHelper));
    }

    public async Task<IBaseResponse<Token>> Handle(NewRefreshTokenQuery request, CancellationToken ct)
    {
        var oldRefreshToken = _cookieHelper.GetRefreshTokenFromCookie();

        if (string.IsNullOrEmpty(oldRefreshToken))
        {
            return GetValidationErrorResponse();
        }

        var user = await _tokenHelper.GetUserByRefreshToken(oldRefreshToken);
        
        _unitOfWork.RefreshTokenBlacklistRepository.Add(new RefreshTokenBlacklist { RefreshToken = oldRefreshToken });
        await _unitOfWork.Commit(ct);
        
        if (user == null) return GetValidationErrorResponse();

        var newRefreshToken = await _tokenHelper.GenerateNewRefreshToken(user);
        
        user.RefreshTokenExpirationTime = DateTime.Now.ToUniversalTime();
        await _unitOfWork.Commit(ct);
        
        _cookieHelper.SetRefreshTokenInCookie(newRefreshToken);

        return new BaseResponse<Token>
        {
            StatusCode = 200,
            Data = null
        };
    }

    private static ErrorResponse<Token> GetValidationErrorResponse() => new()
    {
        StatusCode = 400,
        Errors = new Dictionary<string, List<string>>{{"ValidationError", new List<string> {"Refresh token not valid"}}},
    };
}
