using System.ComponentModel.DataAnnotations;

namespace ASISYA.DTOs.Product
{
    // DTO para actualizar un producto (PUT)
    public class ProductUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        public int CategoryID { get; set; }

        public int? SupplierID { get; set; }

        [MaxLength(50)]
        public string? QuantityPerUnit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        public decimal? UnitPrice { get; set; }

        [Range(0, short.MaxValue)]
        public short? UnitsInStock { get; set; }

        [Range(0, short.MaxValue)]
        public short? UnitsOnOrder { get; set; }

        [Range(0, short.MaxValue)]
        public short? ReorderLevel { get; set; }

        public bool Discontinued { get; set; }
    }

}
