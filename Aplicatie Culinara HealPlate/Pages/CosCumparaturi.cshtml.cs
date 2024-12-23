using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using Tesseract;
using TesseractOCR.Enums;
using System.Globalization;

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
        public static string NormalizeText(string text)
        {
            if (text == null)
                return string.Empty;

            // Convertește la litere mici
            text = text.ToLowerInvariant();

            // Elimină diacriticele
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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

            /*if (CosCumparaturi == null || !CosCumparaturi.CosIngredientes.Any())
            {
                // Dacă nu există coș sau coșul este gol, redirecționează către o pagină adecvată
                return RedirectToPage("/CosCumparaturi"); // Pagina cu un mesaj că coșul este gol
            }*/

            // Returnăm pagina cu datele coșului
            return Page();
        }

        // Metodă pentru eliminarea unui ingredient din coș
        public async Task<IActionResult> OnPostEliminaDinCosAsync(int id)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (idUtilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }

            // Verificăm dacă ingredientul există în coșul utilizatorului
            var cosIngrediente = await _context.CosIngredientes
                .Include(ci => ci.IdCosNavigation)
                .FirstOrDefaultAsync(ci => ci.IdIngredient == id && ci.IdCosNavigation.IdUtilizator == idUtilizator);

            if (cosIngrediente == null)
            {
                return NotFound();
            }
            Console.WriteLine($"Șterg ingredientul cu ID {id}");

            // Eliminăm înregistrarea din baza de date
            _context.CosIngredientes.Remove(cosIngrediente);
            await _context.SaveChangesAsync();

            // Redirecționăm utilizatorul înapoi la pagina coșului
            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostUploadImageAsync(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Te rugăm să încarci o imagine validă.");
                return Page();
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, image.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            try
            {
                // Configurare Tesseract
                using var ocrEngine = new TesseractEngine("./tessdata", "ron", Tesseract.EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                var result = ocrEngine.Process(img);

                var etichetaText = result.GetText();
               // Console.WriteLine($"Textul din etichetă: {etichetaText}");

                // Normalizează textul etichetei
                var etichetaTextNormalizat = NormalizeText(etichetaText);
                Console.WriteLine($"Textul din etichetă: {etichetaTextNormalizat}");

                // Obține restricțiile utilizatorului
                var restrictii = await GetRestrictiiUtilizatorAsync();

                // Preluăm lista de ingrediente din baza de date
                var ingrediente = await _context.Ingredientes
                    .Include(i => i.IngredientAlergenis)
                        .ThenInclude(ia => ia.IdAlergenNavigation)
                    .ToListAsync();

                // Listă pentru alergeni identificați
                var alergeniDetectati = new List<string>();

                // Verificăm textul etichetei pentru ingrediente și alergeni
                foreach (var ingredient in ingrediente)
                {
                    var ingredientNormalizat = NormalizeText(ingredient.Nume);

                    if (etichetaTextNormalizat.Contains(ingredientNormalizat, StringComparison.OrdinalIgnoreCase))
                    {
                        // Verificăm dacă ingredientul conține alergeni în lista de restricții
                        var alergeniAsociati = ingredient.IngredientAlergenis
                            .Where(ia => restrictii.Contains(ia.IdAlergenNavigation.NumeAlergen, StringComparer.OrdinalIgnoreCase))
                            .Select(ia => ia.IdAlergenNavigation.NumeAlergen)
                            .ToList();

                        alergeniDetectati.AddRange(alergeniAsociati);
                        
                    }
                }
                if (alergeniDetectati.Any())
                {
                    ViewData["ScanResult"] = $"Produsul conține alergeni nepermisi: {string.Join(", ", alergeniDetectati)}.";
                }
                else
                {
                    ViewData["ScanResult"] = "Produsul este compatibil cu restricțiile tale alimentare.";
                }
            }
            catch (Exception ex)
            {
                ViewData["ScanResult"] = $"A apărut o eroare la procesarea imaginii: {ex.Message}";
            }

            // Preluăm din nou coșul de cumpărături al utilizatorului
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == idUtilizator);

            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }

            CosCumparaturi = await _context.CosuriDeCumparaturis
                .Include(c => c.CosIngredientes)
                .ThenInclude(ci => ci.IdIngredientNavigation)
                .Where(c => c.IdUtilizator == idUtilizator)
                .FirstOrDefaultAsync();

            return Page();
        }


        // Funcție pentru preluarea restricțiilor utilizatorului din baza de date
        private async Task<List<string>> GetRestrictiiUtilizatorAsync()
        {
            // Obține ID-ul utilizatorului curent din sesiune
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            // Găsește utilizatorul și include alergeni prin relațiile corespunzătoare
            var utilizator = await _context.Utilizatoris
                .Include(u => u.UtilizatorAlergenis)
                    .ThenInclude(ua => ua.IdAlergenNavigation) // Include tabela Alergeni
                .FirstOrDefaultAsync(u => u.IdUtilizator == idUtilizator);

            // Returnează lista de nume ale alergenilor
            return utilizator?.UtilizatorAlergenis
                .Select(ua => ua.IdAlergenNavigation.NumeAlergen)
                .ToList() ?? new List<string>();
        }
    }
}
