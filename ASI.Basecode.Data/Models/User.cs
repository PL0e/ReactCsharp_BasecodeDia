namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
