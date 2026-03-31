namespace ASI.Basecode.WebApp.Models.Auth
{
    public class SetPasswordRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
