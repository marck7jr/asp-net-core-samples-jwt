namespace AspNetCoreSamplesJwt.Contracts
{
    public interface IJwtService
    {
        public object GetJwtToken<T>(T user) where T : IUserCredentials, IUserInfo;

    }
}
