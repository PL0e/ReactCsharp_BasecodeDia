namespace ASI.Basecode.WebApp.Models.Admin
{
    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public int? YearLevelId { get; set; }
    }
}
