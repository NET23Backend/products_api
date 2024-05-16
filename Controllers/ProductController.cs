using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_API_Products.Data;
using REST_API_Products.Models;

namespace REST_API_Products.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductController : ControllerBase

	{
		private readonly ApplicationDbContext _context;

		public ProductController(ApplicationDbContext context)
		{
			_context = context;
		}

		[HttpPost]
		public async Task<ActionResult<Product>> AddProduct(Product product)
		{
			_context.Products.Add(product);
			await _context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<Product>> GetProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);

			if(product == null)
			{
				return NotFound();
			}

			return product;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
		{
			return await _context.Products
				.Select(p => new ProductDTO
				{
					Id = p.Id,
					Name = p.Name,
					Price = p.Price
				})
				.ToListAsync();
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> EditProduct(int id, Product product)
		{
			if (id != product.Id)
			{
				return BadRequest();
			}

			_context.Entry(product).State = EntityState.Modified;

			try
			{
				await _context.SaveChangesAsync();
			} catch (DbUpdateConcurrencyException)
			{
				if (!ProductExists(id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);

			if (product == null)
			{
				return NotFound();
			}

			_context.Products.Remove(product);
			await _context.SaveChangesAsync();

			return NoContent();
		}

		// privat helper klass
		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.Id == id);
		}

    }
}




































































