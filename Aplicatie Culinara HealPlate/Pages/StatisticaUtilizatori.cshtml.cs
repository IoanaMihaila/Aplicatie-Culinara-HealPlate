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
        }
    }
}
