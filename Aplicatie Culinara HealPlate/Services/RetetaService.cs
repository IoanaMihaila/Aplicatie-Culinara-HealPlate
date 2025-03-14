using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.EntityFrameworkCore;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public class RetetaService : IRetetaService//implementarea serviciului
    {
        private readonly HealPlateDbContext _context;

        public RetetaService(HealPlateDbContext context)
        {
            _context = context;
        }
        public Retete GetRetetaById(int id)
        {
            var reteta = _context.Retetes
                .Include(r => r.Recenziis)  // Include recenziile asociate rețetei
                .Include(r => r.RetetaIngredientes)  // Include ingredientele asociate rețetei
                    .ThenInclude(ri => ri.IdIngredientNavigation)  // Include ingredientul pentru fiecare ingredient din rețetă
                    .ThenInclude(i => i.IngredientAlergenis)// Include alergeni pentru ingredient
                    .ThenInclude(ia => ia.IdAlergenNavigation)
                    .AsSplitQuery()
                .FirstOrDefault(r => r.IdReteta == id);

            return reteta;
        }
        public async Task<bool> ApprovePost1Async(int idReteta)
        {
            var reteta = await _context.Retetes.FindAsync(idReteta);

            if (reteta == null)
            {
                return false; // Rețeta nu a fost găsită
            }

            reteta.Aprobata = true;
            _context.Retetes.Update(reteta);
            var result = await _context.SaveChangesAsync();

            return result > 0; // Dacă modificarea a avut succes
        }
        public async Task<bool> RejectPostAsync(int idReteta)
        {
            var reteta = await _context.Retetes.FindAsync(idReteta);

            if (reteta == null)
            {
                return false; // Rețeta nu a fost găsită
            }

            reteta.Aprobata = false; // Se marchează ca respinsă (sau poți șterge în loc de a marca)
            _context.Retetes.Update(reteta);
            var result = await _context.SaveChangesAsync();

            return result > 0; // Dacă modificarea a avut succes
        }

    }
}
