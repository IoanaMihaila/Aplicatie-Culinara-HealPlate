using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class VizualizareReteteModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        public VizualizareReteteModel(HealPlateDbContext context)
        {
            _context = context;
        }
        public List<Retete> Retete { get; set; } = new List<Retete>();
        public Dictionary<int, bool> EsteInColectie { get; set; } = new();
        public List<string> Categorii { get; set; } = new List<string> { "Toate", "Mic dejun", "Prânz", "Cină", "Desert", "Gustare" };
        public string CategorieSelectata { get; set; } = "Toate";

        // Adăugarea unei rețete în colecția personală
        public async Task<IActionResult> OnPostAddToCollectionAsync([FromBody] int idReteta)
        {
            //Console.WriteLine($"Request received with IdReteta: {request?.IdReteta}");
            //var idReteta = request?.IdReteta;  // Preia id-ul rețetei din cererea JSON
            if (idReteta <= 0)
            {
                return new JsonResult(new { success = false, message = "ID-ul rețetei nu este valid." });
            }
            // Obținem ID-ul utilizatorului curent
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);

            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }

            // Verificăm dacă utilizatorul are deja o colecție personală
            var colectie = await _context.ColectiePersonalas
                .FirstOrDefaultAsync(c => c.IdUtilizator == utilizator.IdUtilizator);

            if (colectie == null)
            {
                // Dacă nu există, creăm o colecție personală pentru utilizator
                colectie = new ColectiePersonala
                {
                    IdUtilizator = utilizator.IdUtilizator,
                    DataAdaugare = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.ColectiePersonalas.Add(colectie);
                await _context.SaveChangesAsync();
            }

            // Verificăm dacă rețeta nu este deja în colecția utilizatorului
            var existingFavorite = await _context.ColectiePersonalaRetetes
                .FirstOrDefaultAsync(cr => cr.IdColectie == colectie.IdColectie && cr.IdReteta == idReteta);

            if (existingFavorite != null)
            {
                return new JsonResult(new { success = false, message = "Rețeta este deja în colecție." });
            }

            // Crează sau adaugă rețeta
            var colectieReteta = new ColectiePersonalaRetete
            {
                IdColectie = colectie.IdColectie,
                IdReteta = idReteta
            };

            _context.ColectiePersonalaRetetes.Add(colectieReteta);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }

        public void OnGet(string? categorie = null)
        {
            CategorieSelectata = categorie ?? "Toate";

            // Obține rețetele din baza de date
            IQueryable<Retete> query = _context.Retetes;

            if (!string.IsNullOrEmpty(categorie) && categorie != "Toate")
            {
                query = query.Where(r => r.Categorie == categorie);
            }

            Retete = query.ToList();

            // Obține ID-ul utilizatorului curent din sesiune
            var userId = HttpContext.Session.GetInt32("IdUtilizator");

            if (userId != null)
            {
                // Obține ID-ul colecției personale a utilizatorului
                var idColectie = _context.ColectiePersonalas
                    .Where(c => c.IdUtilizator == userId)
                    .Select(c => c.IdColectie)
                    .FirstOrDefault();

                if (idColectie != 0)
                {
                    // Obține lista ID-urilor rețetelor din colecția personală
                    var reteteInColectie = _context.ColectiePersonalaRetetes
                        .Where(cr => cr.IdColectie == idColectie)
                        .Select(cr => cr.IdReteta)
                        .ToList();

                    // Creează dicționarul pentru a marca rețetele existente în colecție
                    EsteInColectie = Retete.ToDictionary(
                        r => r.IdReteta,
                        r => reteteInColectie.Contains(r.IdReteta)
                    );
                }
            }
        }
        public async Task<IActionResult> OnPostRemoveFromCollectionAsync([FromBody] int idReteta)
        {
            if (idReteta <= 0)
            {
                return new JsonResult(new { success = false, message = "ID-ul rețetei nu este valid." });
            }

            try
            {
                // Obține utilizatorul curent
                var userId = HttpContext.Session.GetInt32("IdUtilizator");  // Presupunem că utilizatorul este autentificat și se poate obține numele său
                ColectiePersonala colectie = await _context.ColectiePersonalas
                    .FirstOrDefaultAsync(c => c.IdUtilizator == userId);
                // Verifică dacă utilizatorul are rețeta în colecție
                var favoriteRecipe = await _context.ColectiePersonalaRetetes
                .FirstOrDefaultAsync(cr => cr.IdColectie == colectie.IdColectie && cr.IdReteta == idReteta);

                if (favoriteRecipe != null)
                {
                    // Șterge rețeta din colecția utilizatorului
                    _context.ColectiePersonalaRetetes.Remove(favoriteRecipe);
                    await _context.SaveChangesAsync();

                    // Returnează un succes
                    return new JsonResult(new { success = true, message = "Rețeta a fost ștearsă din colecția ta." });
                }
                else
                {
                    // Dacă rețeta nu există în colecție
                    return new JsonResult(new { success = false, message = "Rețeta nu există în colecția ta." });
                }
            }
            catch (Exception ex)
            {
                // Loghează eroarea (de exemplu, folosind un logger)
                Console.WriteLine(ex.Message);

                // Returnează un mesaj de eroare
                return new JsonResult(new { success = false, message = "A apărut o eroare la ștergerea rețetei." });
            }
        }

    }
}