using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ASISYA.Data;
using ASISYA.Models;
using ASISYA.DTOs.Product;

namespace ASISYA.Controllers.product
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {

        private readonly AppDBContext _context;

        public ProductsController(AppDBContext context)
        {
            _context = context;
        }


        // Mapea la entidad Product -> ProductDto
        private static ProductDto ToDto(Product p) => new ProductDto
        {
            ProductID = p.ProductID,
            ProductName = p.ProductName,
            QuantityPerUnit = p.QuantityPerUnit,
            UnitPrice = p.UnitPrice,
            UnitsInStock = p.UnitsInStock,
            UnitsOnOrder = p.UnitsOnOrder,
            ReorderLevel = p.ReorderLevel,
            Discontinued = p.Discontinued,
            CategoryID = p.CategoryID,
            CategoryName = p.Category?.CategoryName,
            SupplierID = p.SupplierID,
            SupplierName = p.Supplier?.CompanyName
        };


        // GET: api/products
        //[HttpGet]
        [HttpGet("sinpaginacion")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();

            return products.Select(ToDto).ToList();
        }


        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound($"No se encontró el producto con ID {id}.");
            }

            return ToDto(product);
        }


        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductCreateDto dto)
        {
            bool categoriaExiste = await _context.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
            if (!categoriaExiste)
            {
                return BadRequest($"No existe la categoría con ID {dto.CategoryID}.");
            }

            if (dto.SupplierID.HasValue)
            {
                bool proveedorExiste = await _context.Suppliers.AnyAsync(s => s.SupplierID == dto.SupplierID);
                if (!proveedorExiste)
                {
                    return BadRequest($"No existe el proveedor con ID {dto.SupplierID}.");
                }
            }

            var product = new Product
            {
                ProductName = dto.ProductName,
                CategoryID = dto.CategoryID,
                SupplierID = dto.SupplierID,
                QuantityPerUnit = dto.QuantityPerUnit,
                UnitPrice = dto.UnitPrice,
                UnitsInStock = dto.UnitsInStock,
                UnitsOnOrder = dto.UnitsOnOrder,
                ReorderLevel = dto.ReorderLevel,
                Discontinued = dto.Discontinued
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Recargar con relaciones para el DTO de respuesta
            await _context.Entry(product).Reference(p => p.Category).LoadAsync();
            if (product.SupplierID.HasValue)
            {
                await _context.Entry(product).Reference(p => p.Supplier).LoadAsync();
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, ToDto(product));
        }


        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound($"No se encontró el producto con ID {id}.");
            }

            bool categoriaExiste = await _context.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
            if (!categoriaExiste)
            {
                return BadRequest($"No existe la categoría con ID {dto.CategoryID}.");
            }

            if (dto.SupplierID.HasValue)
            {
                bool proveedorExiste = await _context.Suppliers.AnyAsync(s => s.SupplierID == dto.SupplierID);
                if (!proveedorExiste)
                {
                    return BadRequest($"No existe el proveedor con ID {dto.SupplierID}.");
                }
            }

            product.ProductName = dto.ProductName;
            product.CategoryID = dto.CategoryID;
            product.SupplierID = dto.SupplierID;
            product.QuantityPerUnit = dto.QuantityPerUnit;
            product.UnitPrice = dto.UnitPrice;
            product.UnitsInStock = dto.UnitsInStock;
            product.UnitsOnOrder = dto.UnitsOnOrder;
            product.ReorderLevel = dto.ReorderLevel;
            product.Discontinued = dto.Discontinued;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Products.AnyAsync(p => p.ProductID == id))
                {
                    return NotFound($"No se encontró el producto con ID {id}.");
                }
                throw;
            }

            return NoContent();
        }


        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound($"No se encontró el producto con ID {id}.");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }



        //[HttpGet("paginacion")]
        [HttpGet]
        public async Task<ActionResult<PagedResponse<Product>>> GetProductsPage([FromQuery] ProductParameters validFilter)
        {
            // 1. Clonar el IQueryable inicial (no ejecuta la consulta aún)
            var query = _context.Products.AsQueryable();


            // 2. Aplicar Búsqueda (Search)
            /*
            if (!string.IsNullOrWhiteSpace(validFilter.ProductName))
            {
                var search = validFilter.ProductName.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(search));
            }
            */
            // 2. Aplicamos el filtro de búsqueda si el término no está vacío
            if (!string.IsNullOrWhiteSpace(validFilter.SearchTerm))
            {
                var search = validFilter.SearchTerm.Trim().ToLower();

                // Ejemplo buscando por Nombre o Descripción
                query = query.Where(p => p.ProductName.ToLower().Contains(search));
            }



            // 3. Aplicar Filtros (Filters)
            if (!string.IsNullOrWhiteSpace(validFilter.CategoryID))
            {
                query = query.Where(p => p.CategoryID.ToString() == validFilter.CategoryID);
            }

                
                
            // 4. Contar el total de elementos con los filtros aplicados (para la paginación)
            var totalRecords = await query.CountAsync();
            //var totalRecords = 2;

            // 5. Aplicar Paginación (Skip & Take) y ejecutar consulta
            var pagedData = await query
                .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();

            // 6. Retornar respuesta formateada
            return Ok(new PagedResponse<Product>(pagedData, totalRecords, validFilter.PageNumber, validFilter.PageSize));
        }





    }
}
