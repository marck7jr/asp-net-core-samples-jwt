using AspNetCoreSamplesJwt.Contracts;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AspNetCoreSamplesJwt.Models
{
    public class UserCredentials : ObservableObject, IUserCredentials
    {
        private string email;
        private string password;
        private string grantType;
        private string refreshToken;

        public string Email { get => email; set => Set(ref email, value); }
        public string Password { get => password; set => Set(ref password, value); }
        [JsonPropertyName("grant_type")]
        public string GrantType { get => grantType; set => Set(ref grantType, value); }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get => refreshToken; set => Set(ref refreshToken, value); }
    }
}
