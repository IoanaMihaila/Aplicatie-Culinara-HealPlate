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
                    .AsSplitQuery()
                .FirstOrDefault(r => r.IdReteta == id);

            return reteta;
        }
    }
}
