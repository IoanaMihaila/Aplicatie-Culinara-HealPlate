using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Pages;
using Microsoft.EntityFrameworkCore;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public class CosService : ICosService
    {
        private readonly HealPlateDbContext _context;

        public CosService(HealPlateDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> AdaugaIngredientInCosAsync(int userId, AdaugaInCosRequest request)
        {
            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);
            if (utilizator == null)
                return (false, "Utilizatorul nu a fost găsit.");

            var cos = await _context.CosuriDeCumparaturis.FirstOrDefaultAsync(c => c.IdUtilizator == userId);
            if (cos == null)
            {
                cos = new CosuriDeCumparaturi
                {
                    IdUtilizator = utilizator.IdUtilizator,
                    DataCreare = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.CosuriDeCumparaturis.Add(cos);
                await _context.SaveChangesAsync();
            }

            var cosIngredientExistent = await _context.CosIngredientes
                .FirstOrDefaultAsync(ci => ci.IdCos == cos.IdCos && ci.IdIngredient == request.IdIngredient);

            if (cosIngredientExistent != null)
                return (false, "Ingredientul există deja în coș!");

            var cosIngredient = new CosIngrediente
            {
                IdCos = cos.IdCos,
                IdIngredient = request.IdIngredient,
                Cantitate = (decimal)request.Cantitate,
                Unitate = request.Unitate
            };

            _context.CosIngredientes.Add(cosIngredient);
            await _context.SaveChangesAsync();

            return (true, "Ingredient adăugat în coș!");
        }
    }
}
