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
        public async Task<(bool success, string message)> AddToCollectionAsync(int? userId, int idReteta)
        {
            if (idReteta <= 0) return (false, "ID-ul rețetei nu este valid.");

            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);
            if (utilizator == null) return (false, "Utilizatorul nu este autentificat.");

            var colectie = await _context.ColectiePersonalas
                .FirstOrDefaultAsync(c => c.IdUtilizator == utilizator.IdUtilizator);

            if (colectie == null)
            {
                colectie = new ColectiePersonala
                {
                    IdUtilizator = utilizator.IdUtilizator,
                    DataAdaugare = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.ColectiePersonalas.Add(colectie);
                await _context.SaveChangesAsync();
            }

            var exists = await _context.ColectiePersonalaRetetes.FirstOrDefaultAsync(cr => cr.IdColectie == colectie.IdColectie && cr.IdReteta == idReteta);
            if (exists!=null) return (false, "Rețeta este deja în colecție.");

            var colectieReteta = new ColectiePersonalaRetete
            {
                IdColectie = colectie.IdColectie,
                IdReteta = idReteta
            };

            _context.ColectiePersonalaRetetes.Add(colectieReteta);
            await _context.SaveChangesAsync();
            return (true, "Rețeta a fost adăugată în colecție.");
        }

        public async Task<(bool success, string message)> RemoveFromCollectionAsync(int? userId, int idReteta)
        {
            if (idReteta <= 0) return (false, "ID-ul rețetei nu este valid.");

            var colectie = await _context.ColectiePersonalas.FirstOrDefaultAsync(c => c.IdUtilizator == userId);
            var favorite = await _context.ColectiePersonalaRetetes.FirstOrDefaultAsync(cr => cr.IdColectie == colectie.IdColectie && cr.IdReteta == idReteta);

            if (favorite == null) return (false, "Rețeta nu există în colecția ta.");

            _context.ColectiePersonalaRetetes.Remove(favorite);
            await _context.SaveChangesAsync();
            return (true, "Rețeta a fost ștearsă din colecție.");
        }

        public async Task<(bool success, string message)> DeleteRecipeAsync(int idReteta)
        {
            if (idReteta <= 0) return (false, "ID-ul rețetei nu este valid.");

            var reteta = await _context.Retetes.FindAsync(idReteta);
            if (reteta == null) return (false, "Rețeta nu a fost găsită.");

            _context.Retetes.Remove(reteta);
            await _context.SaveChangesAsync();
            return (true, "Rețeta a fost ștearsă cu succes.");
        }

        public async Task<List<Retete>> GetFilteredReteteAsync(int? userId, string? categorie, string? searchQuery)
        {
            IQueryable<Retete> query = _context.Retetes.Where(r => r.Aprobata == true);

            if (!string.IsNullOrEmpty(categorie) && categorie != "Toate")
                query = query.Where(r => r.Categorie == categorie);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                query = query.Where(r => r.Titlu.ToLower().Contains(searchLower) ||
                    _context.RetetaIngredientes
                        .Where(ri => ri.IdReteta == r.IdReteta)
                        .Join(_context.Ingredientes, ri => ri.IdIngredient, i => i.IdIngredient, (ri, i) => i.Nume.ToLower())
                        .Any(nume => nume.Contains(searchLower)));
            }

            if (userId != null)
            {
                var alergeniUtilizator = await _context.UtilizatorAlergenis
                    .Where(au => au.IdUtilizator == userId)
                    .Select(au => au.IdAlergen)
                    .ToListAsync();

                var ingredienteCuAlergeni = await _context.IngredientAlergenis
                    .Where(ia => alergeniUtilizator.Contains(ia.IdAlergen))
                    .Select(ia => ia.IdIngredient)
                    .ToListAsync();

                if (ingredienteCuAlergeni.Any())
                {
                    query = query.Where(r => !_context.RetetaIngredientes
                        .Where(ri => ri.IdReteta == r.IdReteta)
                        .Select(ri => ri.IdIngredient)
                        .Any(id => ingredienteCuAlergeni.Contains(id)));
                }
            }

            return await query.ToListAsync();
        }

        public async Task<Dictionary<int, bool>> GetEsteInColectieAsync(int userId, List<Retete> retete)
        {
            var idColectie = await _context.ColectiePersonalas
                .Where(c => c.IdUtilizator == userId)
                .Select(c => c.IdColectie)
                .FirstOrDefaultAsync();

            var reteteInColectie = await _context.ColectiePersonalaRetetes
                .Where(cr => cr.IdColectie == idColectie)
                .Select(cr => cr.IdReteta)
                .ToListAsync();

            return retete.ToDictionary(r => r.IdReteta, r => reteteInColectie.Contains(r.IdReteta));
        }

        public List<Retete> CautaRetetePeBazaIngredientelor(List<string> ingrediente)
        {
            return _context.Retetes
                .Include(r => r.RetetaIngredientes)
                    .ThenInclude(ri => ri.IdIngredientNavigation)
                .Where(r => r.RetetaIngredientes
                    .Any(ri => ingrediente.Contains(ri.IdIngredientNavigation.Nume.ToLower())))
                .GroupBy(r => r.IdReteta)   
                .Select(g => g.First())        
                .ToList();
        }

        public List<string> GetAlergeniPentruIngredient(string numeIngredient)
        {
            var ingredient = _context.Ingredientes
                .Include(i => i.IngredientAlergenis)
                    .ThenInclude(ia => ia.IdAlergenNavigation)
                .FirstOrDefault(i => i.Nume.ToLower() == numeIngredient.ToLower());

            if (ingredient == null)
                return new List<string>();

            return ingredient.IngredientAlergenis
                .Select(ia => ia.IdAlergenNavigation.NumeAlergen)
                .ToList();
        }
    }
}
