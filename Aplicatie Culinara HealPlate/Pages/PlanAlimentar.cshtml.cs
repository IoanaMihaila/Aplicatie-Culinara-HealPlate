using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.Json;


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
                            IdReteta = retetaSelectata.IdReteta,
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
        public async Task<IActionResult> OnPostSalvarePlanAsync([FromBody] JsonElement request)
        {
            Console.WriteLine($"JSON primit: {request}");

            if (!request.TryGetProperty("dataSelectata", out JsonElement dataSelectataElement))
            {
                return BadRequest("Nu s-a găsit câmpul dataSelectata în JSON.");
            }

            if (!request.TryGetProperty("retete", out JsonElement reteteElement) || reteteElement.ValueKind != JsonValueKind.Array)
            {
                return BadRequest("Nu s-a găsit câmpul retete sau formatul este incorect.");
            }

            string dataSelectata = dataSelectataElement.GetString();
            Console.WriteLine($"Data extrasă: {dataSelectata}");

            if (string.IsNullOrWhiteSpace(dataSelectata))
            {
                return BadRequest("Data este goală sau invalidă.");
            }

            if (!DateOnly.TryParseExact(dataSelectata, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOnlyValue))
            {
                return BadRequest("Formatul datei este invalid.");
            }

            Console.WriteLine($"Ziua convertită cu succes: {dateOnlyValue}");

            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);

            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }

            var planExistent = await _context.PlanAlimentars
                .FirstOrDefaultAsync(p => p.IdUtilizator == userId && p.Ziua == dateOnlyValue);

            if (planExistent != null)
            {
                return new JsonResult(new { error = "Planul alimentar pentru această zi a fost deja generat." });
            }

            var categorii = new Dictionary<string, (int Id, string Titlu)>();

            foreach (var retetaJson in reteteElement.EnumerateArray())
            {
                if (retetaJson.TryGetProperty("categorie", out JsonElement categorieElement) &&
                    retetaJson.TryGetProperty("idReteta", out JsonElement idRetetaElement) &&
                    retetaJson.TryGetProperty("titlu", out JsonElement titluElement) &&
                    idRetetaElement.TryGetInt32(out int idReteta))
                {
                    string categorie = categorieElement.GetString();
                    string titlu = titluElement.GetString();

                    if (!string.IsNullOrEmpty(categorie) && !string.IsNullOrEmpty(titlu))
                    {
                        categorii[categorie] = (idReteta, titlu);
                    }
                }
            }

            // Crearea obiectului `PlanAlimentar`
            var planNou = new PlanAlimentar
            {
                IdUtilizator = userId,
                IdMicDeJun = categorii.ContainsKey("Mic Dejun") ? categorii["Mic Dejun"].Id : null,
                IdPranz = categorii.ContainsKey("Prânz") ? categorii["Prânz"].Id : null,
                IdCina = categorii.ContainsKey("Cină") ? categorii["Cină"].Id : null,
                IdDesert = categorii.ContainsKey("Desert") ? categorii["Desert"].Id : null,
                IdGustare = categorii.ContainsKey("Gustare") ? categorii["Gustare"].Id : null,
                Ziua = dateOnlyValue
            };

            _context.PlanAlimentars.Add(planNou);
            int rowsAffected = await _context.SaveChangesAsync();

            if (rowsAffected == 0)
            {
                return new JsonResult(new { error = "Nicio modificare nu a fost salvată în baza de date." });
            }

            // Construire mesaj email cu titlurile rețetelor în loc de ID-uri
            var mesajEmail = $@"
<h2>Plan alimentar generat</h2>
<p>Ai generat un plan alimentar pentru data de <strong>{dataSelectata}</strong>.</p>
<p>Rețetele incluse:</p>
<ul>
    {string.Join("", categorii.Select(r => $"<li>{r.Value.Titlu} - Categorie: {r.Key}</li>"))}
</ul>
<p>Un reminder îți va fi trimis în ziua planificată.</p>";

            await _emailService.SendEmailAsync(utilizator.Email, "Plan alimentar generat", mesajEmail);

            return new JsonResult(new { success = "Planul alimentar a fost salvat cu succes!", retete = categorii, ziua = planNou.Ziua });

        }
    }
}
