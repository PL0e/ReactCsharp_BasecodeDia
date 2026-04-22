using System;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class AdminUserResponse
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAdminUserResponse
    {
        public string Message { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public bool IsFirstLogin { get; set; }
    }

    public class MessageResponse
    {
        public string Message { get; set; }
    }
}
