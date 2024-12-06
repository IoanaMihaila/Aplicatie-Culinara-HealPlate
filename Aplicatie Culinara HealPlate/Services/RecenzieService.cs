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
        public async Task DeleteRecenzieAsync(int idRecenzie)
        {
            try
            {
                var recenzie = await _context.Recenziis.FindAsync(idRecenzie);

                // Verifică dacă recenzia există
                if (recenzie != null)
                {
                    _context.Recenziis.Remove(recenzie);
                    Console.WriteLine("Recenzia a fost eliminată, acum salvăm modificările...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Modificările au fost salvate cu succes.");
                }
                else
                {
                    // Poți adăuga un mesaj de logare pentru debugging
                    Console.WriteLine($"Recenzie cu id {idRecenzie} nu a fost găsită.");
                }
            }
            catch (Exception ex)
            {
                // Capturăm orice excepție și o logăm sau o gestionăm corespunzător
                Console.WriteLine($"A apărut o eroare la ștergerea recenziei: {ex.Message}");
                throw; // Opțional, dacă vrei să propagi excepția mai departe
            }
        }
        public async Task UpdateRecenzieAsync(int idRecenzie, string textNou, int scorNou)
        {
            var recenzie = await _context.Recenziis.FindAsync(idRecenzie);
            if (recenzie != null)
            {
                recenzie.TextRecenzie = textNou;
                recenzie.Scor = scorNou;
                await _context.SaveChangesAsync();
            }
        }
    }
}
