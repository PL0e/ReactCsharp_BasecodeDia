namespace ASI.Basecode.WebApp.Models.Api
{
    public class UserSummaryResponse
    {
        public int UserId { get; set; }
        public string StudentId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
    }
}
