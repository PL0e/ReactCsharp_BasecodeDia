namespace ASI.Basecode.WebApp.Models.Admin
{
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public int? YearLevelId { get; set; }
    }
}
