using AspNetCoreSamplesJwt.Contracts;
using AspNetCoreSamplesJwt.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

namespace AspNetCoreSamplesJwt.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public (IJwtAccessTokenData accessTokenData, IJwtRefreshTokenData refreshTokenData) GetJwtTokens<T>(T user) where T : IUserAccount
        {
            var identity = GetClaimsIdentity(user);
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = identity,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddSeconds(double.Parse(_configuration["Jwt:Expires:Access"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SigningCredentials:Key"])), SecurityAlgorithms.HmacSha256Signature)
            });
            var jwtAccessToken = handler.WriteToken(securityToken);
            var jwtRefreshToken = jwtAccessToken.GetKeyDerivationHash();

            return
            (
                new JwtAccessTokenData
                {
                    AccessToken = jwtAccessToken,
                    RefreshToken = jwtRefreshToken,
                    Type = "bearer",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(double.Parse(_configuration["Jwt:Expires:Access"]))
                }, new JwtRefreshTokenData
                {
                    RefreshToken = jwtRefreshToken,
                    Subject = user.Guid,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(double.Parse(_configuration["Jwt:Expires:Refresh"]))
                }
            );
        }

        internal ClaimsIdentity GetClaimsIdentity<T>(T user) where T : IUserAccount
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

    public class JwtTokenData : IJwtTokenData
    {
        [JsonPropertyName("expires_at")]
        public DateTime ExpiresAt { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }

    public class JwtAccessTokenData : JwtTokenData, IJwtAccessTokenData
    {
        [Key, JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("token_type")]
        public string Type { get; set; }
    }

    public class JwtRefreshTokenData : JwtTokenData, IJwtRefreshTokenData
    {
        [Key, JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonPropertyName("subject")]
        public Guid Subject { get; set; }
    }
}
