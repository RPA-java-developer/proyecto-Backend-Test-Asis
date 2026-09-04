using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASISYA.Models
{
    [Table("Suppliers")]
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

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
        public string? HomePage { get; set; }

        // Relación 1:N con Products
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
