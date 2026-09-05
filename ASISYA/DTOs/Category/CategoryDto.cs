namespace ASISYA.DTOs.Category
{
    // DTO para mostrar una categoría (respuesta de GET)
    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? Picture { get; set; }
        public int ProductCount { get; set; }
    }

}
