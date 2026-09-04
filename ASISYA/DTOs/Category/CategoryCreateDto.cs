using System.ComponentModel.DataAnnotations;

namespace ASISYA.DTOs.Category
{
    // DTO para crear una categoría (POST)
    public class CategoryCreateDto
    {
        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public string? Picture { get; set; }
    }


}
