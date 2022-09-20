using System.ComponentModel.DataAnnotations;

namespace SPaPS.Models.AccountModels
{
    public class RegisterModel
    {
        [Display(Name = "E-маил"), Required]
        public string? Email { get; set; }
        [Display(Name = "Телефонски Број"), Required]
        public string? PhoneNumber { get; set; }
        [Display(Name = "Тип на Клиент"), Required]
        public int ClientTypeId { get; set; }
        [Display(Name = "Име и Презиме"), Required]
        public string Name { get; set; } = null!;
        [Display(Name = "Адреса"), Required]
        public string Address { get; set; } = null!;
        [Display(Name = "ИД број"), Required]
        public string IdNo { get; set; } = null!;
        [Display(Name = "Град"), Required]
        public int CityId { get; set; }
        [Display(Name = "Држава"), Required]
        public int? CountryId { get; set; }
    }
}
