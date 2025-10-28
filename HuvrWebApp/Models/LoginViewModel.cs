using System.ComponentModel.DataAnnotations;

namespace HuvrWebApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Client ID is required")]
        [Display(Name = "Client ID")]
        [EmailAddress(ErrorMessage = "Client ID must be a valid email format")]
        public string ClientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Client Secret is required")]
        [Display(Name = "Client Secret")]
        [DataType(DataType.Password)]
        public string ClientSecret { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }
}
