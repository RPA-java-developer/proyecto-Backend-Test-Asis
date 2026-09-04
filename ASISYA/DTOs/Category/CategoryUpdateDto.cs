using System.ComponentModel.DataAnnotations;

namespace ASISYA.DTOs.Category
{
    // DTO para actualizar una categoría (PUT)
    public class CategoryUpdateDto
    {
        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        public string? Picture { get; set; }
    }

}
