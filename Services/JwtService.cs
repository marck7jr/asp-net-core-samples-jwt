using AspNetCoreSamplesJwt.Contracts;
using AspNetCoreSamplesJwt.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace AspNetCoreSamplesJwt.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public object GetJwtToken<T>(T user) where T : IUserCredentials, IUserInfo
        {
            user.Password = user.Password.GetKeyDerivationHash(Encoding.UTF8.GetBytes(user.Email));
            var identity = GetClaimsIdentity(user);
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = identity,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:Expires:Hours"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningCredentials:Key"])), SecurityAlgorithms.HmacSha256Signature)
            });
            var jwtToken = handler.WriteToken(securityToken);

            return new
            {
                access_token = jwtToken,
                token_type = "bearer",
                expires_at = DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:Expires:Hours"]))
            };
        }

        internal ClaimsIdentity GetClaimsIdentity<T>(T user) where T : IUserCredentials, IUserInfo
        {
            return new ClaimsIdentity
            (
                new GenericIdentity(user.Email), new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, user.Guid.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.MobilePhone, user.MobilePhone ?? string.Empty),
                }
            );
        }
    }
}
