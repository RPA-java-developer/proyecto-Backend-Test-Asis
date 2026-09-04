namespace ASISYA.Controllers.product
{
    public class ProductParameters
    {

        // Búsqueda y Filtros

        public string? SearchTerm { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryID { get; set; }


        // Paginación
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;


    }
}
