using System;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class AdviserResponse
    {
        public int AdviserId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }

    public class AdviserDirectoryResponse
    {
        public int AdviserId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }

    public class UpsertAdviserRequest
    {
        public int UserId { get; set; }
    }
}
