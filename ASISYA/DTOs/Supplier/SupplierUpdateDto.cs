using System.ComponentModel.DataAnnotations;

namespace ASISYA.DTOs.Supplier
{// DTO para actualizar un proveedor (PUT)
    public class SupplierUpdateDto
    {
        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ContactName { get; set; }

        [MaxLength(50)]
        public string? ContactTitle { get; set; }

        [MaxLength(150)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? City { get; set; }

        [MaxLength(50)]
        public string? Region { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(50)]
        public string? Country { get; set; }

        [MaxLength(50)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Fax { get; set; }

        [MaxLength(255)]
        [Url(ErrorMessage = "HomePage debe ser una URL válida.")]
        public string? HomePage { get; set; }
    }
}
