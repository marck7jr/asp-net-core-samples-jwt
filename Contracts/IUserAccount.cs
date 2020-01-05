using System;

namespace AspNetCoreSamplesJwt.Contracts
{
    public interface IUserAccount : IUserCredentials, IUserInfo
    {
        public Guid Guid { get; set; }
    }

    public interface IUserCredentials
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public interface IUserInfo
    {
        public string Name { get; set; }
        public string MobilePhone { get; set; }
        public string Role { get; set; }
    }
}
