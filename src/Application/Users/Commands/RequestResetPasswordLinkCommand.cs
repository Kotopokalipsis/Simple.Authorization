using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Interfaces.Application.Responses;
using Application.Common.Interfaces.Infrastructure.Services;
using Application.Common.Responses;
using Ardalis.GuardClauses;
using Domain.Entities;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Users.Commands
{
    public record RequestResetPasswordLinkCommand : IRequest<IBaseResponse<string>>
    {
        public string Email { get; init; }
    }
    
    public class RequestResetPasswordLinkHandler : IRequestHandler<RequestResetPasswordLinkCommand, IBaseResponse<string>>
    {
        private readonly UserManager<User> _userManager;
        private readonly IMailSender _sender;

        public RequestResetPasswordLinkHandler(UserManager<User> userManager, IMailSender sender)
        {
            _sender = Guard.Against.Null(sender, nameof(sender));;
            _userManager = Guard.Against.Null(userManager, nameof(userManager));
        }

        public async Task<IBaseResponse<string>> Handle(RequestResetPasswordLinkCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user.IsTransient())
            {
                return new RollbackTransactionErrorResponse<string>
                {
                    StatusCode = 404,
                    Errors = new Dictionary<string, List<string>>{{"ForgotPassword", new List<string> {"Not Found"}}},
                };
            }
            
            _sender.Send(user.Email, user.UserName, "Reset Password", "this is link to reset password");
            
            return new BaseResponse<string>
            {
                StatusCode = 200,
                Data = "Success"
            };
        }
    }
}