using ASISYA.Data;
using ASISYA.DTOs;
using ASISYA.DTOs.Category;
using ASISYA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASISYA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public CategoriesController(AppDBContext context)
        {
            _context = context;
        }

        private static CategoryDto ToDto(Category c) => new CategoryDto
        {
            CategoryID = c.CategoryID,
            CategoryName = c.CategoryName,
            Description = c.Description,
            ProductCount = c.Products?.Count ?? 0
        };

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return categories.Select(ToDto).ToList();
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound($"No se encontró la categoría con ID {id}.");
            }

            return ToDto(category);
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateDto dto)
        {
            var category = new Category
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description,
                Picture = dto.Picture
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryID }, ToDto(category));
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryUpdateDto dto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound($"No se encontró la categoría con ID {id}.");
            }

            category.CategoryName = dto.CategoryName;
            category.Description = dto.Description;
            category.Picture = dto.Picture;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Categories.AnyAsync(c => c.CategoryID == id))
                {
                    return NotFound($"No se encontró la categoría con ID {id}.");
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound($"No se encontró la categoría con ID {id}.");
            }

            bool tieneProductos = await _context.Products.AnyAsync(p => p.CategoryID == id);
            if (tieneProductos)
            {
                return BadRequest("No se puede eliminar la categoría porque tiene productos asociados.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
