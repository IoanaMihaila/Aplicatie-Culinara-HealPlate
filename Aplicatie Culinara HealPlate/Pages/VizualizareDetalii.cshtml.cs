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
        [BindProperty]
        public int Scor { get; set; }
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
                Scor = Scor,
                IdUtilizator = utilizator.IdUtilizator
            };
            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }
            if (string.IsNullOrEmpty(RecenzieText) || Scor < 1 || Scor > 5)
            {
                ErrorMessage = "Recenzia trebuie să aibă un text valid și un scor între 1 și 5.";
                return RedirectToPage("./VizualizareDetalii", new { id });
            }
            _recenzieService.AddRecenzieAsync(recenzie);
            // Redirecționăm către aceleași detalii ale rețetei
            return RedirectToPage("./VizualizareDetalii", new { id = id });
        }
        public async Task<IActionResult> OnPostStergereRecenzieAsync(int id)
        {
            Console.WriteLine("Am ajuns in metoda OnPostStergereRecenzie");
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare");
            }

            var recenzie = _context.Recenziis
                .FirstOrDefault(r => r.IdReteta == id && r.IdUtilizator == idUtilizator.Value);

            if (recenzie == null)
            {
                return NotFound();
            }

            _context.Recenziis.Remove(recenzie);
            // Salvează modificările în baza de date
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Loghează eroarea dacă există
                Console.WriteLine($"Eroare la salvarea modificărilor: {ex.Message}");
                return StatusCode(500, "A apărut o eroare la salvarea modificărilor.");
            }

                return RedirectToPage("./VizualizareDetalii", new { id = id});
        }
        public async Task<IActionResult> OnPostEditareRecenzieAsync(int id, string textNou, int scorNou)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare");
            }

            var recenzie = _context.Recenziis
                .FirstOrDefault(r => r.IdRecenzie == id && r.IdUtilizator == idUtilizator.Value);

            if (recenzie == null)
            {
                return NotFound();
            }

            recenzie.TextRecenzie = textNou;
            recenzie.Scor = scorNou;

            await _context.SaveChangesAsync();

            return RedirectToPage("./VizualizareDetalii", new { id = recenzie.IdReteta });
        }
    }
}
