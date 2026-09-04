using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASISYA.Models
{
    [Table("Categories")]
    public class Category
    {

        [Key]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        // /ruta/completa/a/la/foto.jpg
        [MaxLength(255)]
        public string? Picture { get; set; }

        // Relación 1:N con Products
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
