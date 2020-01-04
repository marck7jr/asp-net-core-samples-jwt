using System;

namespace AspNetCoreSamplesJwt.Contracts
{
    public interface IUserInfo
    {
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string MobilePhone { get; set; }
        public string Role { get; set; }
    }
}