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

        // asynkrona metoder: async betyder att den kan köra icke-blockerande operationer
        // och frigöra serverresurser medan operationen väntar på att slutföras.
        // vi använder await ihop med async metoder för att vänta på att asynkrona
        // operationer ska slutföras. det förbättrar applikationens responsivitet och
        // skalbarhet genom att frigöra trådar som kan användas för andra förfrågningar.

        // fördelar med async operationer
        // skalbarhet: genom att använda asynkrona metoder kan en applikation hantera fler
        // förfrågningar samtidigt, eftersom trådar inte blockeras medan de väntar på I/O-operationer.

        // responsivitet: användning av await förbättrar applikationens responsivitet,
        // speciellt under långdragna operationer som databasfrågor.

        // prestanda: det frigör resurser som kan användas för att hantera andra
        // förfrågningar medan operationen väntar på att slutföras.


        // Task:
        // representerar en pågående eller framtida operation. asynkrona metoder i C# returnerar
        // en Task eller Task<T>. när du returnerar en Task från en metod, anger du att metoden är asynkron och
        // kan köra operationer utan att blockera den aktuella tråden.


        [HttpPost]
		public async Task<ActionResult<Product>> AddProduct(Product product)
        // Task<ActionResult<Product>>: metoden returnerar ett Task som
        // representerar en asynkron operation. ActionResult<Product> är typen
        // av resultatet när Task är slutfört.
        {
            _context.Products.Add(product);
			await _context.SaveChangesAsync();
            // _context.SaveChangesAsync(): detta är en asynkron version av SaveChanges()
            // eftersom detta kan vara en långdragen operation (beroende på nätverks-
            // och databashastighet), är det bra praxis att göra detta asynkront för att
            // förbättra skalbarheten och responsiviteten hos applikationen.

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            // CreatedAtAction: detta är en metod som returnerar en 201 Created-statuskod
            // tillsammans med en länk till den nyskapade resursen (produkten).

            // nameof(GetProduct): detta är namnet på GET metoden vi har för en enskild produkt.
            // det används för att generera en URI till den nyskapade resursen.

            // new { id = product.Id }: detta skapar ett anonymt objekt med produktens ID som används
            // för att generera URI:en.

            // product: detta är själva produktobjektet som kommer att inkluderas i svaret.
        }

		[HttpGet("{id}")]
		public async Task<ActionResult<Product>> GetProduct(int id)
		
		{
			{
				var product = await _context.Products.FindAsync(id);

				if (product == null)
				{
					return NotFound();
				}

				return product;
			}
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        // IEnumerable<T> representerar en sekvens av element som kan itereras över.
        {
            return await _context.Products
                // .Select(p => new ProductDTO { ... }): är en LINQ-projektion som omvandlar
                // varje Product-objekt till ett ProductDTO-objekt. det används för att skapa en ny
                // anonym typ med endast de fält som behövs.
                .Select(p => new ProductDTO
				{
					Id = p.Id,
					Name = p.Name,
					Price = p.Price
				})
				.ToListAsync();
            // .ToListAsync(): är en asynkron version av ToList(). 
        }

        // PUT
        // fullständig uppdatering: en PUT-förfrågan används för att uppdatera en resurs fullständigt.
        // när du skickar en PUT-förfrågan, skickar du en fullständig representation av resursen.
        // om några fält saknas i förfrågan, antas de vara avsedda att raderas eller nollställas i resursen.

        // PATCH
        // partiell uppdatering: en PATCH-förfrågan används för att göra partiella uppdateringar
        // av en resurs. när du skickar en PATCH-förfrågan, skickar du bara de fält som du vill
        // uppdatera, inte hela resursen.

        // när använda vad?
        // PUT
        // använd när du behöver göra en fullständig uppdatering av en resurs.
        // om du vill säkerställa att resursens alla fält definieras och uppdateras, även om vissa fält
        // sätts till null eller standardvärden.

        // PATCH
        // använd när du bara behöver göra en partiell uppdatering av en resurs.
        // om du vill uppdatera endast specifika fält utan att påverka andra fält i resursen.


        [HttpPut("{id}")]
		public async Task<IActionResult> EditProduct(int id, Product product)
        // <IActionResult> kan vi använda när vi vill kunna returnera olika resultat tex BadRequest(), NoContent() osv
        {
            if (id != product.Id)
            // kontrollerar om ID: t från URL: en matchar ID: t på produktobjektet.
            // det här är en säkerhetskontroll för att säkerställa att klienten inte försöker ändra
            // ID:t på produkten.
            {
                return BadRequest();
                // om ID:na inte matchar, returnerar metoden en 400 Bad Request HTTP-statuskod till klienten
            }

            _context.Entry(product).State = EntityState.Modified;
            // _context.Entry(product): hämtar EntityEntry för det angivna produktobjektet. EntityEntry ger
            // tillgång till metadata om entiteten.

            // State = EntityState.Modified: markerar entiteten som modifierad. detta indikerar för Entity Framework
            // att entiteten har ändrats och att uppdateringar ska göras i databasen när SaveChangesAsync() anropas.

            try
            {
				await _context.SaveChangesAsync();
			} catch (DbUpdateConcurrencyException)
            // catch (DbUpdateConcurrencyException): fångar ett DbUpdateConcurrencyException som uppstår
            // om två användare samtidigt försöker uppdatera samma produkt.
            {
                if (!ProductExists(id))
				{
					return NotFound();
                    // om produkten har tagits bort av en annan användare returnerar metoden en 404 Not
                    // Found HTTP-statuskod.
                }
                else
				{
					throw;
                    // om produkten fortfarande existerar och ett annat problem orsakade undantaget,
                    // kastar det om undantaget för vidare hantering.
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




































































