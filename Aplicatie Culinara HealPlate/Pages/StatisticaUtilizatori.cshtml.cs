using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class StatisticaUtilizatoriModel : PageModel
    {
        private readonly HealPlateDbContext _context;

        public StatisticaUtilizatoriModel(HealPlateDbContext context)
        {
            _context = context;
        }

        public List<Utilizatori> Utilizatori { get; private set; } = new();
        public Dictionary<string, int> StatisticaAlergeni { get; private set; } = new();
        public Retete CeaMaiApreciataReteta { get; private set; }
        public int NumarColecții { get; private set; }
        public Dictionary<string, int> StatisticaRetete { get; private set; } = new();
        public Dictionary<string, int> IngredienteFrecventeUtilizator { get; private set; } = new();
        public Dictionary<string, int> ReteteFrecventeUtilizator { get; private set; } = new();
        public int TimpGatireSaptamanal { get; private set; }
        public Dictionary<string, int> EvolutiePreferinte { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (idUtilizator == null)
                return;

            // Obține lista utilizatorilor și alergenii lor
            Utilizatori = await _context.Utilizatoris
                .Include(u => u.UtilizatorAlergenis)
                    .ThenInclude(ua => ua.IdAlergenNavigation)
                .Include(u => u.ColectiePersonala)
                    .ThenInclude(cp => cp.ColectiePersonalaRetetes)
                        .ThenInclude(cpr => cpr.IdRetetaNavigation)
                .Include(u => u.PlanAlimentars)
                    .ThenInclude(p => p.IdMicDeJunNavigation)
                .Include(u => u.PlanAlimentars)
                    .ThenInclude(p => p.IdPranzNavigation)
                .Include(u => u.PlanAlimentars)
                    .ThenInclude(p => p.IdCinaNavigation)
                .Include(u => u.PlanAlimentars)
                    .ThenInclude(p => p.IdGustareNavigation)
                .Include(u => u.PlanAlimentars)
                    .ThenInclude(p => p.IdDesertNavigation)
                .ToListAsync();

            var utilizator = await _context.Utilizatoris
    .Include(u => u.PlanAlimentars)
        .ThenInclude(p => p.IdMicDeJunNavigation)
    .Include(u => u.PlanAlimentars)
        .ThenInclude(p => p.IdPranzNavigation)
    .Include(u => u.PlanAlimentars)
        .ThenInclude(p => p.IdCinaNavigation)
    .Include(u => u.PlanAlimentars)
        .ThenInclude(p => p.IdGustareNavigation)
    .Include(u => u.PlanAlimentars)
        .ThenInclude(p => p.IdDesertNavigation)
    .FirstOrDefaultAsync(u => u.IdUtilizator == idUtilizator);


            // Generează statistica alergiilor (numărul de utilizatori afectați de fiecare alergen)
            StatisticaAlergeni = await _context.UtilizatorAlergenis
                .GroupBy(ua => ua.IdAlergenNavigation.NumeAlergen)
                .Select(group => new { Alergen = group.Key, Count = group.Count() })
                .OrderByDescending(g => g.Count)
                .ToDictionaryAsync(g => g.Alergen, g => g.Count);
            // Generează statistica rețetelor (numărul de utilizatori care au salvat fiecare rețetă)
            StatisticaRetete = await _context.ColectiePersonalaRetetes
                .GroupBy(cr => cr.IdRetetaNavigation.Titlu)  // Grupăm după titlul rețetei
                .Select(group => new { Reteta = group.Key, Count = group.Count() })
                .OrderByDescending(g => g.Count)
                .ToDictionaryAsync(g => g.Reteta, g => g.Count);

            // Obține rețeta cea mai apreciată
            CeaMaiApreciataReteta = await _context.Retetes
                .OrderByDescending(r => r.ColectiePersonalaRetetes.Count)  // Alege rețeta cu cele mai multe apariții
                .FirstOrDefaultAsync();

            // Poți adăuga și numărul total de colecții
            NumarColecții = await _context.ColectiePersonalas.CountAsync();

            // 🔍 Statistici nutriționale personale
            var colectieUtilizator = await _context.ColectiePersonalas
                .Include(c => c.ColectiePersonalaRetetes)
                    .ThenInclude(cpr => cpr.IdRetetaNavigation)
                        .ThenInclude(r => r.RetetaIngredientes)
                            .ThenInclude(ri => ri.IdIngredientNavigation)
                .FirstOrDefaultAsync(c => c.IdUtilizator == idUtilizator);

            if (colectieUtilizator != null)
            {
                // 2. Ingrediente cele mai frecvente
                IngredienteFrecventeUtilizator = colectieUtilizator.ColectiePersonalaRetetes
                    .SelectMany(cpr => cpr.IdRetetaNavigation.RetetaIngredientes)
                    .GroupBy(ri => ri.IdIngredientNavigation.Nume)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count());

                // 4. Evoluția preferințelor în timp (opțional)
                EvolutiePreferinte = colectieUtilizator.ColectiePersonalaRetetes
                    .GroupBy(cpr => cpr.IdColectieNavigation.DataAdaugare)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count());
            }
            if (utilizator != null)
            {
                var startOfWeek = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday));

                TimpGatireSaptamanal = utilizator.PlanAlimentars
                    .Where(p => p.Ziua >= startOfWeek && p.Ziua <= startOfWeek.AddDays(6))
                    .SelectMany(p => new List<Retete?> {
                        p.IdMicDeJunNavigation,
                        p.IdPranzNavigation,
                        p.IdCinaNavigation,
                        p.IdGustareNavigation,
                        p.IdDesertNavigation
                    })
                    .Where(r => r != null)
                    .Sum(r => r!.TimpPreparare);

                var toateReteteleDinPlanuri = utilizator.PlanAlimentars
        .SelectMany(p => new List<Retete?> {
            p.IdMicDeJunNavigation,
            p.IdPranzNavigation,
            p.IdCinaNavigation,
            p.IdGustareNavigation,
            p.IdDesertNavigation
        })
        .Where(r => r != null)
        .GroupBy(r => r!.Titlu)
        .ToDictionary(g => g.Key, g => g.Count());

    ReteteFrecventeUtilizator = toateReteteleDinPlanuri
        .OrderByDescending(kv => kv.Value)
        .Take(5)
        .ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }
    }
}
