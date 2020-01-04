using AspNetCoreSamplesJwt.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreSamplesJwt.Models
{
    public class UserAccount : ObservableObject, IUserCredentials, IUserInfo
    {
        private Guid guid;
        private string name;
        private string email;
        private string password;
        private string mobilePhone;
        private string role;

        [Key]
        public Guid Guid { get => guid; set => Set(ref guid, value); }
        public string Name { get => name; set => Set(ref name, value); }
        [Required, EmailAddress]
        public string Email { get => email; set => Set(ref email, value); }
        [Required]
        public string Password { get => password; set => Set(ref password, value); }
        [Phone]
        public string MobilePhone { get => mobilePhone; set => Set(ref mobilePhone, value); }
        public string Role { get => role; set => Set(ref role, value); }
    }
}
