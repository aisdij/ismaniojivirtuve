using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Backend.Server.Database.Tables
{
    [Table("UserInfo")]
    public class UserInfoTable
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string PasswordHashed { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string? Gender { get; set; }

        public string? Role { get; set; }

        public int Points { get; set; }

        public DateTime? LastTimeWheelSpin { get; set; }
    }
}
