using ASISYA.Data;
using ASISYA.Models;
using ASISYA.DTOs.Supplier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASISYA.Controllers.supplier
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDBContext _context;

        public SuppliersController(AppDBContext context)
        {
            _context = context;
        }

        private static SupplierDto ToDto(Supplier s) => new SupplierDto
        {
            SupplierID = s.SupplierID,
            CompanyName = s.CompanyName,
            ContactName = s.ContactName,
            ContactTitle = s.ContactTitle,
            Address = s.Address,
            City = s.City,
            Region = s.Region,
            PostalCode = s.PostalCode,
            Country = s.Country,
            Phone = s.Phone,
            Fax = s.Fax,
            HomePage = s.HomePage,
            ProductCount = s.Products?.Count ?? 0
        };

        // GET: api/suppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
        {
            var suppliers = await _context.Suppliers
                .Include(s => s.Products)
                .ToListAsync();

            return suppliers.Select(ToDto).ToList();
        }

        // GET: api/suppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierID == id);

            if (supplier == null)
            {
                return NotFound($"No se encontró el proveedor con ID {id}.");
            }

            return ToDto(supplier);
        }

        // POST: api/suppliers
        [HttpPost]
        public async Task<ActionResult<SupplierDto>> CreateSupplier(SupplierCreateDto dto)
        {
            var supplier = new Supplier
            {
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                ContactTitle = dto.ContactTitle,
                Address = dto.Address,
                City = dto.City,
                Region = dto.Region,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                Phone = dto.Phone,
                Fax = dto.Fax,
                HomePage = dto.HomePage
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierID }, ToDto(supplier));
        }

        // PUT: api/suppliers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, SupplierUpdateDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound($"No se encontró el proveedor con ID {id}.");
            }

            supplier.CompanyName = dto.CompanyName;
            supplier.ContactName = dto.ContactName;
            supplier.ContactTitle = dto.ContactTitle;
            supplier.Address = dto.Address;
            supplier.City = dto.City;
            supplier.Region = dto.Region;
            supplier.PostalCode = dto.PostalCode;
            supplier.Country = dto.Country;
            supplier.Phone = dto.Phone;
            supplier.Fax = dto.Fax;
            supplier.HomePage = dto.HomePage;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Suppliers.AnyAsync(s => s.SupplierID == id))
                {
                    return NotFound($"No se encontró el proveedor con ID {id}.");
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/suppliers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound($"No se encontró el proveedor con ID {id}.");
            }

            bool tieneProductos = await _context.Products.AnyAsync(p => p.SupplierID == id);
            if (tieneProductos)
            {
                return BadRequest("No se puede eliminar el proveedor porque tiene productos asociados.");
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
