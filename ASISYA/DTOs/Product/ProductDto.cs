namespace ASISYA.DTOs.Product
{
    // DTO para mostrar un producto (respuesta de GET)
    public class ProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool Discontinued { get; set; }

        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }

        public int? SupplierID { get; set; }
        public string? SupplierName { get; set; }
    }

}