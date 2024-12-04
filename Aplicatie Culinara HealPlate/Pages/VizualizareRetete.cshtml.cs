using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        public List<string> Categorii { get; set; } = new List<string> { "Toate", "Mic dejun", "Prânz", "Cină", "Desert", "Gustare" };
        public string CategorieSelectata { get; set; } = "Toate";
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
        }
    }
}
