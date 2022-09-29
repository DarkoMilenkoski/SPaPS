using System.ComponentModel.DataAnnotations;

namespace SPaPS.Models.CustomModels
{
    public class vm_Service
    {
        public long ServiceId { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public List<long> ActivityIds { get; set; } = new List<long>();
        public string? Activities { get; set; }
    }
}
