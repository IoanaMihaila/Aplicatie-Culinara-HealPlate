using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public class RecenzieService : IRecenzieService
    {
        private readonly HealPlateDbContext _context;
        public RecenzieService(HealPlateDbContext context)
        {
            _context = context;
        }

        public async Task AddRecenzieAsync(Recenzii recenzie)
        {
            _context.Recenziis.Add(recenzie);
            await _context.SaveChangesAsync();
        }
        public Recenzii GetRecenzieByUtilizatorSiReteta(int idUtilizator, int idReteta)
        {
            // Căutăm o recenzie pentru utilizatorul specific și rețeta specifică
            return _context.Recenziis
                .FirstOrDefault(r => r.IdUtilizator == idUtilizator && r.IdReteta == idReteta);
        }
    }
}
