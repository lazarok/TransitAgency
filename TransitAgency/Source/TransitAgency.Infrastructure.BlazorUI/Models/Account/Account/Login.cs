using System.ComponentModel.DataAnnotations;

namespace TransitAgency.Infrastructure.BlazorUI.Models.Account
{
    public class Login
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}