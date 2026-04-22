using System;

namespace ASI.Basecode.WebApp.Models.Auth
{
    public class ForgotPasswordStartRequest
    {
        public string Email { get; set; }
    }

    public class ForgotPasswordStartResponse
    {
        public string Message { get; set; }
        public string Username { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
