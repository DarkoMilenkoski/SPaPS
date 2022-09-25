using System.ComponentModel.DataAnnotations;

namespace SPaPS.Models.AccountModels
{
    public class ChangePasswordModel
    {
        [Required]
        public string OldPassword { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        [Compare(nameof(NewPassword), ErrorMessage = "Лозинките не се совпаѓаат!"), Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
