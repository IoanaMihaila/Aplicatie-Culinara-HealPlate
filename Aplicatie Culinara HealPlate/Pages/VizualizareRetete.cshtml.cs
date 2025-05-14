using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Newtonsoft.Json;
using OneOf.Types;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class VizualizareReteteModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        private readonly IRetetaService _retetaService;
        public VizualizareReteteModel(HealPlateDbContext context, IRetetaService retetaService)
        {
            _context = context;
            _retetaService = retetaService;
        }
        public List<Retete> Retete { get; set; } = new List<Retete>();
        public string SearchQuery { get; set; }
        public Dictionary<int, bool> EsteInColectie { get; set; } = new();
        public List<string> Categorii { get; set; } = new List<string> { "Toate", "Mic dejun", "Prânz", "Cină", "Desert", "Gustare" };
        public string CategorieSelectata { get; set; } = "Toate";

        // Adăugarea unei rețete în colecția personală
        public async Task<IActionResult> OnPostAddToCollectionAsync([FromBody] int idReteta)
        {
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            var (success, message) = await _retetaService.AddToCollectionAsync(userId, idReteta);
            return new JsonResult(new { success, message });
        }

        public void OnGet(string? categorie = null, string? searchQuery = null)
        {
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            Retete = _retetaService.GetFilteredReteteAsync(userId, categorie, searchQuery).Result;

            if (userId != null)
            {
                EsteInColectie = _retetaService.GetEsteInColectieAsync(userId.Value, Retete).Result;
            }
        }
        public async Task<IActionResult> OnPostRemoveFromCollectionAsync([FromBody] int idReteta)
        {
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            var (success, message) = await _retetaService.RemoveFromCollectionAsync(userId, idReteta);
            return new JsonResult(new { success, message });
        }
        public async Task<IActionResult> OnPostDeleteRecipeAsync([FromBody] int idReteta)
        {
            var (success, message) = await _retetaService.DeleteRecipeAsync(idReteta);
            return new JsonResult(new { success, message });
        }
    }
}