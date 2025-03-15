using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class PlanAlimentarModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        private readonly IEmailService _emailService;

        public PlanAlimentarModel(HealPlateDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public List<Retete> ReteteAlese { get; set; } = new List<Retete>();
        [HttpPost]
        public async Task<IActionResult> OnPostGenerarePlanAsync()
        {
            try
            {
                // Obține ID-ul utilizatorului autentificat
                var userId = HttpContext.Session.GetInt32("IdUtilizator");

                // Definim categoriile pentru planul alimentar
                var categorii = new List<string> { "Mic Dejun", "Prânz", "Cină", "Desert", "Gustare" };
                var reteteGenerate = new List<Retete>();

                // Obține lista ID-urilor alergenilor utilizatorului
                var alergeniUtilizator = await _context.UtilizatorAlergenis
                    .Where(au => au.IdUtilizator == userId)
                    .Select(au => au.IdAlergen)
                    .ToListAsync();

                // Obține ID-urile ingredientelor care conțin acei alergeni
                var ingredienteCuAlergeni = await _context.IngredientAlergenis
                    .Where(ia => alergeniUtilizator.Contains(ia.IdAlergen))
                    .Select(ia => ia.IdIngredient)
                    .ToListAsync();

                // Extragem toate rețetele aprobate care NU conțin acele ingrediente
                var toateRetetele = await _context.Retetes
                    .Where(r => r.Aprobata == true && !_context.RetetaIngredientes
                        .Where(ri => ri.IdReteta == r.IdReteta)
                        .Select(ri => ri.IdIngredient)
                        .Any(idIngredient => ingredienteCuAlergeni.Contains(idIngredient)))
                    .ToListAsync();

                if (toateRetetele == null || toateRetetele.Count == 0)
                {
                    Console.WriteLine("Nu există rețete în baza de date.");
                    return new JsonResult(new { error = "Nu există rețete disponibile." });
                }

                // Selectăm aleatoriu câte o rețetă pentru fiecare categorie
                foreach (var categorie in categorii)
                {
                    var reteteCategorie = toateRetetele.Where(r => r.Categorie == categorie).ToList();

                    if (reteteCategorie.Any())
                    {
                        var retetaSelectata = reteteCategorie.OrderBy(r => Guid.NewGuid()).First();
                        reteteGenerate.Add(new Retete
                        {
                            Titlu = retetaSelectata.Titlu,
                            Imagine = retetaSelectata.Imagine,
                            Categorie = retetaSelectata.Categorie,
                            Descriere = retetaSelectata.Descriere
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Nu există rețete pentru categoria: {categorie}");
                    }
                }

                // Log pentru verificare
                Console.WriteLine("Rețete generate:");
                foreach (var r in reteteGenerate)
                {
                    Console.WriteLine($"Titlu: {r.Titlu}, Categorie: {r.Categorie}");
                }

                return new JsonResult(reteteGenerate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la generarea planului alimentar: {ex.Message}");
                return new JsonResult(new { error = "Eroare internă la generarea planului." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> OnPostSalvarePlanAsync([FromForm] DateOnly ziuaSelectata)
        {
            Console.WriteLine($"Data primită: {ziuaSelectata}");
            try
            {
                // Obține ID-ul utilizatorului autentificat
                var userId = HttpContext.Session.GetInt32("IdUtilizator");
                var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);

                if (utilizator == null)
                {
                    return RedirectToPage("/Autentificare");
                }

                var categorii = new List<string> { "Mic Dejun", "Prânz", "Cină", "Desert", "Gustare" };
                var reteteGenerate = new Dictionary<string, Retete>();

                // Extrage toate rețetele o singură dată
                var toateRetetele = await _context.Retetes.Where(r => r.Aprobata == true).ToListAsync();

                if (!toateRetetele.Any())
                {
                    return new JsonResult(new { error = "Nu există rețete disponibile." });
                }

                // Selectează o rețetă aleatorie pentru fiecare categorie
                foreach (var categorie in categorii)
                {
                    var reteteCategorie = toateRetetele.Where(r => r.Categorie == categorie).ToList();
                    if (reteteCategorie.Any())
                    {
                        reteteGenerate[categorie] = reteteCategorie.OrderBy(r => Guid.NewGuid()).First();
                    }
                }

                // Verifică dacă există deja un plan alimentar pentru ziua curentă
                var planExistent = await _context.PlanAlimentars
                    .FirstOrDefaultAsync(p => p.IdUtilizator == userId && p.Ziua == ziuaSelectata);

                if (planExistent != null)
                {
                    return new JsonResult(new { error = "Planul alimentar pentru azi a fost deja generat." });
                }

                // Salvează planul alimentar în baza de date
                var planNou = new PlanAlimentar
                {
                    IdUtilizator = userId,
                    IdMicDeJun = reteteGenerate.ContainsKey("Mic Dejun") ? reteteGenerate["Mic Dejun"].IdReteta : null,
                    IdPranz = reteteGenerate.ContainsKey("Prânz") ? reteteGenerate["Prânz"].IdReteta : null,
                    IdCina = reteteGenerate.ContainsKey("Cină") ? reteteGenerate["Cină"].IdReteta : null,
                    IdDesert = reteteGenerate.ContainsKey("Desert") ? reteteGenerate["Desert"].IdReteta : null,
                    IdGustare = reteteGenerate.ContainsKey("Gustare") ? reteteGenerate["Gustare"].IdReteta : null,
                    Ziua = ziuaSelectata
                };

                _context.PlanAlimentars.Add(planNou);
                int rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected == 0)
                {
                    return new JsonResult(new { error = "Nicio modificare nu a fost salvată în baza de date." });
                }
                // Construiește mesajul email
                var mesajEmail = $@"
            <h2>Plan alimentar generat</h2>
            <p>Ai generat un plan alimentar pentru data de <strong>{ziuaSelectata}</strong>.</p>
            <p>Rețetele incluse:</p>
            <ul>
                {string.Join("", reteteGenerate.Values.Select(r => $"<li>{r.Titlu} ({r.Categorie})</li>"))}
            </ul>
            <p>Un reminder îți va fi trimis în ziua planificată.</p>";

                // Trimite email
                await _emailService.SendEmailAsync(utilizator.Email, "Plan alimentar generat", mesajEmail);
                return new JsonResult(new { success = "Planul alimentar a fost salvat cu succes!", retete = reteteGenerate.Values });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = "Eroare la salvarea planului alimentar.", details = ex.Message });
            }
        }
    }
}
