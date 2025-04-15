using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class RaportNutritieModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        private static readonly string apiKey = "7fb61485a0mshe687d00c1901b51p1306c1jsnae721f78f3d2";
        private static readonly string apiUrl = "https://health-calculator-api.p.rapidapi.com";

        public RaportNutritieModel(HealPlateDbContext context)
        {
            _context = context;
        }

        public List<PlanAlimentar> PlanuriAlimentare { get; set; } = new();

        public async Task OnGet()
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            var rolUtilizator = HttpContext.Session.GetString("Rol");

            IQueryable<PlanAlimentar> query = _context.PlanAlimentars
                .Include(p => p.IdMicDeJunNavigation)
                .Include(p => p.IdPranzNavigation)
                .Include(p => p.IdDesertNavigation)
                .Include(p => p.IdGustareNavigation)
                .Include(p => p.IdCinaNavigation);

            if (rolUtilizator == "Utilizator" && idUtilizator != null)
            {
                query = query.Where(p => p.IdUtilizator == idUtilizator);
            }

            PlanuriAlimentare = await query.ToListAsync();
        }
        [HttpPost]
        public async Task<IActionResult> OnPostGenerareRaport([FromBody] JsonElement request)
        {
            Console.WriteLine($"Planuri alimentare găsite: {PlanuriAlimentare?.Count ?? 0}");

            Console.WriteLine($"JSON primit: {request}");

            if (!request.TryGetProperty("dataSelectata", out JsonElement dataSelectataElement))
            {
                return BadRequest("Nu s-a găsit câmpul dataSelectata în JSON.");
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

            var plan = await _context.PlanAlimentars
            .Include(p => p.IdMicDeJunNavigation).ThenInclude(r => r.RetetaIngredientes).ThenInclude(ri => ri.IdIngredientNavigation)
            .Include(p => p.IdPranzNavigation).ThenInclude(r => r.RetetaIngredientes).ThenInclude(ri => ri.IdIngredientNavigation)
            .Include(p => p.IdDesertNavigation).ThenInclude(r => r.RetetaIngredientes).ThenInclude(ri => ri.IdIngredientNavigation)
            .Include(p => p.IdGustareNavigation).ThenInclude(r => r.RetetaIngredientes).ThenInclude(ri => ri.IdIngredientNavigation)
            .Include(p => p.IdCinaNavigation).ThenInclude(r => r.RetetaIngredientes).ThenInclude(ri => ri.IdIngredientNavigation)
            .FirstOrDefaultAsync(p => p.Ziua == dateOnlyValue);

            if (plan == null)
            {
                return BadRequest("Nu există plan alimentar pentru această zi.");
            }

            // 2. Obține lista de ingrediente
            var ingrediente = plan.IdMicDeJunNavigation?.RetetaIngredientes
                .Concat(plan.IdPranzNavigation?.RetetaIngredientes ?? Enumerable.Empty<RetetaIngrediente>())
                .Concat(plan.IdDesertNavigation?.RetetaIngredientes ?? Enumerable.Empty<RetetaIngrediente>())
                .Concat(plan.IdGustareNavigation?.RetetaIngredientes ?? Enumerable.Empty<RetetaIngrediente>())
                .Concat(plan.IdCinaNavigation?.RetetaIngredientes ?? Enumerable.Empty<RetetaIngrediente>())
                .Select(ri => new { ri.IdIngredientNavigation.Nume, ri.Cantitate, ri.Unitate })
                .ToList();

            if (!ingrediente.Any())
            {
                return BadRequest("Nu există ingrediente pentru această zi.");
            }

            // 3. Obține informațiile nutriționale pentru fiecare ingredient
            var nutritionInfo = new List<string>();
            foreach (var ingredient in ingrediente)
            {
                var info = await ObtineInfoNutritionala(ingredient.Nume);
                nutritionInfo.Add(info);
            }

            // 4. Generează raportul nutrițional
            var raport = new
            {
                Ingrediente = ingrediente,
                DetaliiNutriționale = nutritionInfo
            };

            return new JsonResult(raport);
        }
        private async Task<string> ObtineInfoNutritionala(string ingredient)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);

            // URL-ul pentru cererea GET
            var url = $"{apiUrl}/{ingredient}";

            try
            {
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return $"Eroare la obținerea datelor pentru ingredientul {ingredient}.";
                }

                var data = await response.Content.ReadAsStringAsync();
                return data; // Răspunsul în JSON (poți să-l procesezi ulterior)
            }
            catch (Exception ex)
            {
                return $"Eroare la cererea NutriAPI pentru ingredientul {ingredient}: {ex.Message}";
            }
        }
    }
}
