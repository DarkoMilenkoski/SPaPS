using Microsoft.Build.Framework;

namespace SPaPS.Models.AccountModels
{
    public class ForgotPasswordModel
    {
        [Required]
        public string Email { get; set; }
    }
}
