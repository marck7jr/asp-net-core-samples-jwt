using System;

namespace AspNetCoreSamplesJwt.Contracts
{
    public interface IJwtService
    {
        public (IJwtAccessTokenData accessTokenData, IJwtRefreshTokenData refreshTokenData) GetJwtTokens<T>(T user) where T : IUserAccount;
    }

    public interface IJwtTokenData
    {
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public interface IJwtAccessTokenData : IJwtTokenData
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Type { get; set; }
    }

    public interface IJwtRefreshTokenData : IJwtTokenData
    {
        public string RefreshToken { get; set; }
        public Guid Subject { get; set; }
    }

}
