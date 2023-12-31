﻿using Authentications.GeneratedServices;
using EasyMicroservices.Cores.Interfaces;
using EasyMicroservices.IdentityMicroservice.Contracts.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace EasyMicroservices.IdentityMicroservice.Helpers
{
    public class ClaimManager
    {
        private List<Claim> claims = new();

        public ClaimManager(IHttpContextAccessor httpContext)
        {

            _httpContext = httpContext;


            var token = _httpContext.HttpContext?.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                claims = jwtToken.Claims.ToList();
            }

        }

        IHttpContextAccessor _httpContext;

        public bool HasId()
        {
            return claims.Any(x => x.Type == "Id");
        }

        public long Id
        {
            get
            {
                return long.Parse(claims.FirstOrDefault(x => x.Type == "Id")?.Value);
            }
        }

        public string CurrentLanguage
        {
            get
            {
                return claims.FirstOrDefault(x => x.Type == "CurrentLanguage")?.Value;
            }
        }

        public List<string> Role
        {
            get
            {
                return claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
            }
        }

        public string UniqueIdentity
        {
            get
            {
                return claims.FirstOrDefault(x => x.Type == "UniqueIdentity")?.Value;
            }
        }

        public List<ClaimContract> SetCurrentLanguage(string value, List<ClaimContract> claims = default)
        {
            claims ??= new List<ClaimContract>();
            if (value != null)
                claims.Add(new ClaimContract
                {
                    Name = "CurrentLanguage",
                    Value = value
                });

            return claims;
        }

        public List<ClaimContract> SetId(long? value, List<ClaimContract> claims = default)
        {
            claims ??= new List<ClaimContract>();
            if (value.HasValue)
                claims.Add(new ClaimContract
                {
                    Name = "Id",
                    Value = value.Value.ToString()
                });

            return claims;
        }

        public List<ClaimContract> SetRole(ICollection<ClaimContract> roles, List<ClaimContract> claims = default)
        {
            claims ??= new List<ClaimContract>();
            if (roles.HasAny())
                foreach (ClaimContract role in roles)
                {
                    claims.Add(new ClaimContract
                    {
                        Name = role.Name,
                        Value = role.Value.ToString()
                    });
                }
            return claims;
        }

        public List<ClaimContract> SetUniqueIdentity(string value, List<ClaimContract> claims = default)
        {
            claims ??= new List<ClaimContract>();
            if (value != null)
                claims.Add(new ClaimContract
                {
                    Name = nameof(IUniqueIdentitySchema.UniqueIdentity),
                    Value = value
                });
            return claims;
        }
    }
}
