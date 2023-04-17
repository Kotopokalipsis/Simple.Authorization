﻿using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Application.Common.Interfaces.Infrastructure.Services
{
    public interface IJwtReader
    {
        IEnumerable<Claim> GetClaimsFromToken(string token);
        DateTime GetValidTo(string token);
    }
}