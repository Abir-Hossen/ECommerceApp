using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.ViewModels
{
    public class AdminProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public decimal? Price { get; set; }

        [Required]
        public int? Stock { get; set; }
    }
}
