using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Common.Interfaces.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.JwtGenerator
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly DateTime _accessTokenExpires = DateTime.Now.AddHours(1);
        private readonly DateTime _refreshTokenExpires = DateTime.Now.AddSeconds(30);

        private readonly SymmetricSecurityKey _accessKey;
        private readonly SymmetricSecurityKey _refreshKey;

        public JwtGenerator(IConfiguration configuration)
        {
            _accessKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Identity:AccessApiKey"]));
            _refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Identity:RefreshApiKey"]));
        }

        public string CreateAccessToken(Guid id)
        {
            return CreateToken(id, _accessTokenExpires, _accessKey);
        }

        public string CreateRefreshToken(Guid id)
        {
            return CreateToken(id, _refreshTokenExpires, _refreshKey);
        }

        private static string CreateToken(Guid id, DateTime expires, SecurityKey key)
        {
            var claims = new List<Claim> { new(JwtRegisteredClaimNames.Jti, id.ToString()) };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = credentials
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}