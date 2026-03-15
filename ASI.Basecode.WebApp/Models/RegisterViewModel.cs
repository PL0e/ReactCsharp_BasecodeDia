using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    public class RegisterViewModel
    {
        [JsonPropertyName("userId")]
        [Required(ErrorMessage = "UserId is required.")]
        public string UserId { get; set; }

        [JsonPropertyName("name")]
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [JsonPropertyName("password")]
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; }

        [JsonPropertyName("confirmPassword")]
        [Required(ErrorMessage = "ConfirmPassword is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
