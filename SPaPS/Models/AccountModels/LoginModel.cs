using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SPaPS.Models.AccountModels
{
    public class LoginModel
    {
        [DisplayName("Кориснички име"), Required]
        public string Email { get; set; }
        [DisplayName("Лозинка"), Required]
        public string Password { get; set; }
    }
}
