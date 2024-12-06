using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;
using Aplicatie_Culinara_HealPlate.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class VizualizareDetaliiModel : PageModel
    {
        private readonly IRetetaService _retetaService;
        private readonly HealPlateDbContext _context;
        private readonly IRecenzieService _recenzieService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VizualizareDetaliiModel(IRetetaService retetaService, HealPlateDbContext context, IRecenzieService recenzieService, IHttpContextAccessor httpContextAccessor)
        {
            _retetaService = retetaService;
            _context = context;
            _recenzieService = recenzieService;
            _httpContextAccessor = httpContextAccessor;

        }

        public Retete Reteta { get; set; }
        // Model pentru recenzia introdusă
        [BindProperty]
        public string RecenzieText { get; set; }
        public string ErrorMessage { get; set; }

        public IActionResult OnGet(int id)
        {
            // Căutăm rețeta după ID-ul primit
            Reteta = _retetaService.GetRetetaById(id);

            // Dacă rețeta nu există, returnăm o eroare 404
            if (Reteta == null)
            {
                return NotFound();
            }
            // Verificăm dacă utilizatorul este autentificat
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare"); // Redirect către pagina de login dacă utilizatorul nu este autentificat
            }

            // Verificăm dacă utilizatorul a lăsat deja o recenzie pentru această rețetă
            var utilizator = _context.Utilizatoris.FirstOrDefault(u => u.IdUtilizator == idUtilizator);
            if (utilizator != null)
            {
                var recenzieExistenta = _recenzieService.GetRecenzieByUtilizatorSiReteta(utilizator.IdUtilizator, id);
                if (recenzieExistenta != null)
                {
                    ErrorMessage = "Ai deja o recenzie pentru această rețetă!";
                }
            }


            // Încarcă recenziile cu utilizatorii asociați
            Reteta.Recenziis = _context.Recenziis
                .Include(r => r.IdUtilizatorNavigation)  // Include utilizatorul asociat
                .Where(r => r.IdReteta == id)  // Filtrare după rețeta respectivă
                .ToList();

            // Returnăm pagina cu detaliile rețetei
            return Page();
        }

        // Metoda POST care redirecționează către aceeași pagină
        public IActionResult OnPost(int id)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare"); // Redirect la pagina de login dacă utilizatorul nu este autentificat
            }
            var utilizator = _context.Utilizatoris.FirstOrDefault(u => u.IdUtilizator == idUtilizator.Value);

            // Adăugăm recenzia în baza de date
            var recenzie = new Recenzii
            {
                IdReteta = id,
                TextRecenzie = RecenzieText,
                IdUtilizator = utilizator.IdUtilizator
            };
            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }

            _recenzieService.AddRecenzieAsync(recenzie);
            // Redirecționăm către aceleași detalii ale rețetei
            return RedirectToPage("./VizualizareDetalii", new { id = id });
        }
    }
}
