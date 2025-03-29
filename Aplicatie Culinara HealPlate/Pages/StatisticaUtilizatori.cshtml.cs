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

        public async Task OnGetAsync()
        {
            // Obține lista utilizatorilor și alergenii lor
            Utilizatori = await _context.Utilizatoris
                .Include(u => u.UtilizatorAlergenis)
                .ThenInclude(ua => ua.IdAlergenNavigation)
                .ToListAsync();

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
        }
    }
}
