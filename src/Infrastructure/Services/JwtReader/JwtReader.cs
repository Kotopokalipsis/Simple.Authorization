using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Common.Interfaces.Infrastructure.Services;

namespace Infrastructure.Services.JwtReader
{
    public class JwtReader : IJwtReader
    {
        private readonly JwtSecurityTokenHandler _handler;
        
        public JwtReader()
        {
            _handler = new JwtSecurityTokenHandler();
        }
        
        public DateTime GetValidTo(string token)
        {
            return _handler.ReadJwtToken(token).ValidTo;
        }
        
        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);

            return jwtSecurityToken.Claims;
        }
    }
}