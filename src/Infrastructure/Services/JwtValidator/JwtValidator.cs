using System;
using System.IdentityModel.Tokens.Jwt;
using Application.Common.Interfaces.Infrastructure.Services;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.JwtValidator
{
    public class JwtValidator : IJwtValidator
    {
        public bool Validate(string token, SymmetricSecurityKey key)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                }, out var validatedToken);

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}