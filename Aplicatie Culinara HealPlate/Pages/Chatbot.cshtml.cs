using Aplicatie_Culinara_HealPlate.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Aplicatie_Culinara_HealPlate.Extensions; // asigura-te ca ai creat acest namespace

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class ChatbotModel : PageModel
    {
        private readonly OllamaService _ollamaService;
        private readonly HealPlateDbContext _context;

        public ChatbotModel(OllamaService ollamaService, HealPlateDbContext context)
        {
            _ollamaService = ollamaService;
            _context = context;
        }

        [BindProperty]
        public string Prompt { get; set; }

        public List<(string Intrebare, string Raspuns)> Conversatie { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Prompt))
            {
                return Page();
            }

            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            var esteAutentificat = userId.HasValue;

            string promptFinal = esteAutentificat
                ? $"Utilizator autentificat. Eviți alergeni. Întrebare: {Prompt}"
                : $"Utilizator neautentificat. Sugestii generale. Întrebare: {Prompt}";

            string raspuns = await _ollamaService.GetOllamaResponseAsync(promptFinal);

            if (esteAutentificat)
            {
                var toateIngredientele = await _context.Ingredientes
                    .Select(i => new { i.IdIngredient, Nume = i.Nume.ToLower() })
                    .ToListAsync();

                var ingredienteDetectate = toateIngredientele
                    .Where(i => Prompt.ToLower().Contains(i.Nume))
                    .Select(i => i.IdIngredient)
                    .ToList();

                if (ingredienteDetectate.Any())
                {
                    var alergeniUtilizator = await _context.UtilizatorAlergenis
                        .Where(a => a.IdUtilizator == userId)
                        .Select(a => a.IdAlergen)
                        .ToListAsync();

                    var ingredienteAlergene = await _context.IngredientAlergenis
                        .Where(ia => alergeniUtilizator.Contains(ia.IdAlergen))
                        .Select(ia => ia.IdIngredient)
                        .ToListAsync();

                    var ingredientePericuloase = await _context.Ingredientes
                        .Where(i => ingredienteDetectate.Contains(i.IdIngredient) && ingredienteAlergene.Contains(i.IdIngredient))
                        .Select(i => i.Nume)
                        .ToListAsync();

                    if (ingredientePericuloase.Any())
                    {
                        raspuns += $"\n\n⚠️ Ai menționat ingrediente alergenice: {string.Join(", ", ingredientePericuloase)}.";
                    }

                    var reteteGasite = await _context.RetetaIngredientes
                        .Where(ri => ingredienteDetectate.Contains(ri.IdIngredient)
                                     && !ingredienteAlergene.Contains(ri.IdIngredient))
                        .Select(ri => new
                        {
                            ri.IdReteta,
                            ri.IdRetetaNavigation.Titlu
                        })
                        .Distinct()
                        .ToListAsync();

                    if (reteteGasite.Any())
                    {
                        raspuns += "\n\n📌 Rețete disponibile care conțin aceste ingrediente:\n";
                        foreach (var reteta in reteteGasite)
                        {
                            raspuns += $"- <a href=\"/VizualizareDetalii?id={reteta.IdReteta}\" target=\"_blank\">{reteta.Titlu}</a><br/>";
                        }
                    }
                }
            }
            else
            {
                raspuns += "\n\n🔒 Creează un cont pentru sugestii personalizate și rețete reale! <a href=\"/Inregistrare\" class=\"btn btn-sm btn-success\">Creează cont</a>";
            }

            Conversatie = HttpContext.Session.GetObjectFromJson<List<(string, string)>>("Conversatie") ?? new();
            Conversatie.Add((Prompt, raspuns));
            HttpContext.Session.SetObjectAsJson("Conversatie", Conversatie);

            return Page();
        }

        public void OnGet()
        {
            Conversatie = HttpContext.Session.GetObjectFromJson<List<(string, string)>>("Conversatie") ?? new();
        }
    }
}
