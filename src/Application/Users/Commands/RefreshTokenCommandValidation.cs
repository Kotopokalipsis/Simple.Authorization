using System.Text;
using Application.Common.Interfaces.Infrastructure.Services;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Users.Commands
{
    public class RefreshTokenCommandValidation : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidation(IJwtValidator jwtValidator, IConfiguration configuration)
        {
            RuleFor(x => x.RefreshToken)
                .Must((rootObject, token) 
                    => jwtValidator.Validate(token, new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Identity:RefreshApiKey"]))))
                .WithMessage("Refresh token not valid");
        }
    }
}