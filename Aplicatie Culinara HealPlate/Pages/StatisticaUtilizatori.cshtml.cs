using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
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

        public List<Alergeni> TotiAlergenii { get; set; } = new();
        public List<Alergeni> AlergeniUtilizator { get; set; } = new();


        public async Task OnGetAsync()
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (idUtilizator == null)
                return;

            TotiAlergenii = await _context.Alergenis.ToListAsync();

            if (idUtilizator != null)
            {
                AlergeniUtilizator = await _context.UtilizatorAlergenis
                    .Where(x => x.IdUtilizator == idUtilizator)
                    .Select(x => x.IdAlergenNavigation)
                    .ToListAsync();
            }


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


            StatisticaAlergeni = await _context.UtilizatorAlergenis
                .GroupBy(ua => ua.IdAlergenNavigation.NumeAlergen)
                .Select(group => new { Alergen = group.Key, Count = group.Count() })
                .OrderByDescending(g => g.Count)
                .ToDictionaryAsync(g => g.Alergen, g => g.Count);
           
            StatisticaRetete = await _context.ColectiePersonalaRetetes
                .GroupBy(cr => cr.IdRetetaNavigation.Titlu)  
                .Select(group => new { Reteta = group.Key, Count = group.Count() })
                .OrderByDescending(g => g.Count)
                .ToDictionaryAsync(g => g.Reteta, g => g.Count);

            CeaMaiApreciataReteta = await _context.Retetes
                .OrderByDescending(r => r.ColectiePersonalaRetetes.Count)  
                .FirstOrDefaultAsync();

            NumarColecții = await _context.ColectiePersonalas.CountAsync();

            var colectieUtilizator = await _context.ColectiePersonalas
                .Include(c => c.ColectiePersonalaRetetes)
                    .ThenInclude(cpr => cpr.IdRetetaNavigation)
                        .ThenInclude(r => r.RetetaIngredientes)
                            .ThenInclude(ri => ri.IdIngredientNavigation)
                .FirstOrDefaultAsync(c => c.IdUtilizator == idUtilizator);

            if (colectieUtilizator != null)
            {
                IngredienteFrecventeUtilizator = colectieUtilizator.ColectiePersonalaRetetes
                    .SelectMany(cpr => cpr.IdRetetaNavigation.RetetaIngredientes)
                    .GroupBy(ri => ri.IdIngredientNavigation.Nume)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .ToDictionary(g => g.Key, g => g.Count());

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

        public async Task<IActionResult> OnPostStergeAlergenAsync(int idAlergen)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            if (idUtilizator == null) return Unauthorized();

            var rel = await _context.UtilizatorAlergenis
                .FirstOrDefaultAsync(x => x.IdAlergen == idAlergen && x.IdUtilizator == idUtilizator);
            if (rel != null)
            {
                _context.UtilizatorAlergenis.Remove(rel);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAdaugaAlergenAsync(int idAlergenNou)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            if (idUtilizator == null) return Unauthorized();

            bool exista = await _context.UtilizatorAlergenis
                .AnyAsync(x => x.IdAlergen == idAlergenNou && x.IdUtilizator == idUtilizator);

            if (!exista)
            {
                _context.UtilizatorAlergenis.Add(new UtilizatorAlergeni
                {
                    IdUtilizator = idUtilizator.Value,
                    IdAlergen = idAlergenNou
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

    }
}
