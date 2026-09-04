using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASISYA.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [ForeignKey(nameof(Supplier))]
        public int? SupplierID { get; set; }

        [Required]
        [ForeignKey(nameof(Category))]
        public int CategoryID { get; set; }

        [MaxLength(50)]
        public string? QuantityPerUnit { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        public short? UnitsInStock { get; set; }

        public short? UnitsOnOrder { get; set; }

        public short? ReorderLevel { get; set; }

        public bool Discontinued { get; set; }
        //public object Category { get; internal set; }

        // Propiedades de navegación
        
        public virtual Supplier? Supplier { get; set; }
        public virtual Category? Category { get; set; }
        

    }
}
