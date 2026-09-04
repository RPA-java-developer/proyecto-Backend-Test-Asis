using CsvHelper.Configuration.Attributes;

namespace ASISYA.DTOs.Product
{
    // Debe coincidir con las columnas del CSV (encabezados exactos)
    public class ProductImportRowDto
    {
        [Name("ProductName")]
        public string ProductName { get; set; } = string.Empty;

        [Name("CategoryID")]
        public int CategoryID { get; set; }

        [Name("SupplierID")]
        public int? SupplierID { get; set; }

        [Name("QuantityPerUnit")]
        public string? QuantityPerUnit { get; set; }

        [Name("UnitPrice")]
        public decimal? UnitPrice { get; set; }

        [Name("UnitsInStock")]
        public short? UnitsInStock { get; set; }

        [Name("UnitsOnOrder")]
        public short? UnitsOnOrder { get; set; }

        [Name("ReorderLevel")]
        public short? ReorderLevel { get; set; }

        [Name("Discontinued")]
        public bool Discontinued { get; set; }
    }

}
