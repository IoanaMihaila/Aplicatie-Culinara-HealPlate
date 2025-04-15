using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class ChestionarModel : PageModel
    {
        public List<IntrebareAlergen> Intrebari { get; set; }
        public List<RezultatTestAlergeni> RaspunsuriExistente { get; set; } = new();
        public bool PoateRaspunde { get; set; }

        private readonly HealPlateDbContext _context;

        public ChestionarModel(HealPlateDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            if (idUtilizator == null) return;

            var limita = DateTime.Now.AddDays(-20);

            var rezultateRecente = await _context.RezultatTestAlergenis
                .Where(r => r.IdUtilizator == idUtilizator && r.DataTest >= limita)
                .Include(r => r.IdAlergenNavigation)
                .ToListAsync();

            PoateRaspunde = rezultateRecente.Count == 0;
            RaspunsuriExistente = rezultateRecente;

            if (PoateRaspunde)
            {
                Intrebari = await _context.IntrebareAlergens
                    .Include(i => i.VariantaIntrebareAlergens)
                    .ToListAsync();
            }
        }

        public JsonResult? RezultateJson { get; set; }

        public async Task<IActionResult> OnPostAsync([FromForm] List<int> intrebari)
        {
            int? idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            if (idUtilizator == null)
                return Unauthorized();

            var rezultate = new Dictionary<int, int>(); // key = IdAlergen, value = scor

            foreach (var idVar in intrebari)
            {
                var varianta = await _context.VariantaIntrebareAlergens
                                             .FirstOrDefaultAsync(v => v.IdVarianta == idVar);

                if (varianta != null)
                {
                    if (!rezultate.ContainsKey(varianta.IdAlergenVizat))
                        rezultate[varianta.IdAlergenVizat] = 0;

                    rezultate[varianta.IdAlergenVizat] += varianta.Punctaj;
                }
            }

            var rezultateFinale = new List<object>();

            foreach (var r in rezultate)
            {
                string recomandare;
                if (r.Value >= 3)
                    recomandare = "⚠️ Evită alimentele care conțin acest alergen!";
                else if (r.Value == 2)
                    recomandare = "🔶 Posibilă sensibilitate. Monitorizează consumul.";
                else
                    recomandare = "✅ Fără semne de reacție la acest alergen.";

                _context.RezultatTestAlergenis.Add(new RezultatTestAlergeni
                {
                    IdUtilizator = idUtilizator.Value,
                    IdAlergen = r.Key,
                    Scor = r.Value,
                    DataTest = DateTime.Now,
                    Recomandare = recomandare
                });

                // pentru SweetAlert
                var numeAlergen = await _context.Alergenis
                                                .Where(a => a.IdAlergen == r.Key)
                                                .Select(a => a.NumeAlergen)
                                                .FirstOrDefaultAsync();

                rezultateFinale.Add(new
                {
                    Alergen = numeAlergen,
                    Scor = r.Value,
                    Recomandare = recomandare
                });
            }

            await _context.SaveChangesAsync();

            return new JsonResult(new { succes = true, rezultate = rezultateFinale });
        }

        [BindProperty]
        public List<EditareIntrebareDto> IntrebariEditate { get; set; }

        public async Task<IActionResult> OnPostSalveazaIntrebariAsync(List<IntrebareAlergen> Intrebari)
        {
            foreach (var intrebare in Intrebari)
            {
                var intrebDb = await _context.IntrebareAlergens
                                    .Include(i => i.VariantaIntrebareAlergens)
                                    .FirstOrDefaultAsync(i => i.IdIntrebare == intrebare.IdIntrebare);

                if (intrebDb != null)
                {
                    intrebDb.Text = intrebare.Text;

                    foreach (var variantaNoua in intrebare.VariantaIntrebareAlergens)
                    {
                        var variantaExistenta = intrebDb.VariantaIntrebareAlergens
                            .FirstOrDefault(v => v.IdVarianta == variantaNoua.IdVarianta);

                        if (variantaExistenta != null)
                        {
                            variantaExistenta.Text = variantaNoua.Text;
                            variantaExistenta.Punctaj = variantaNoua.Punctaj;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["Mesaj"] = "Modificările au fost salvate cu succes!";
            return RedirectToPage();
        }
    }

    public class EditareIntrebareDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<EditareVariantaDto> VariantaIntrebari { get; set; }
    }

    public class EditareVariantaDto
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int Punctaj { get; set; }
    }
}
