using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class CosCumparaturiModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        public CosuriDeCumparaturi CosCumparaturi { get; set; }

        public CosCumparaturiModel(HealPlateDbContext context)
        {
            _context = context;
        }
        // Metodă care preia coșul de cumpărături al unui utilizator
        public async Task<IActionResult> OnGetAsync()
        {
            // Preluăm ID-ul utilizatorului din sesiune (asigură-te că ai stocat acest ID la autentificare)
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == idUtilizator);

            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }

            // Obținem coșul de cumpărături al utilizatorului din baza de date
            CosCumparaturi = await _context.CosuriDeCumparaturis
                .Include(c => c.CosIngredientes)
                .ThenInclude(ci => ci.IdIngredientNavigation)
                .Where(c => c.IdUtilizator == idUtilizator)
                .FirstOrDefaultAsync();

            if (CosCumparaturi == null)
            {
                return NotFound(); // Dacă nu există coș pentru utilizator, returnăm NotFound
            }

            // Returnăm pagina cu datele coșului
            return Page();
        }
    }
}
