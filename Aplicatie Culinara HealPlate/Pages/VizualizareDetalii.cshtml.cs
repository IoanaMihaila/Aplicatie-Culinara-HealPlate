using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplicatie_Culinara_HealPlate.Services;
namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class VizualizareDetaliiModel : PageModel
    {
        private readonly IRetetaService _retetaService;

        public VizualizareDetaliiModel(IRetetaService retetaService)
        {
            _retetaService = retetaService;
        }

        public Retete Reteta { get; set; }

        public IActionResult OnGet(int id)
        {
            Console.WriteLine($"ID-ul rețetei este: {id}");
            // Căutăm rețeta după ID-ul primit
            Reteta = _retetaService.GetRetetaById(id);

            // Dacă rețeta nu există, returnăm o eroare 404
            if (Reteta == null)
            {
                return NotFound();
            }

            // Returnăm pagina cu detaliile rețetei
            return Page();
        }
    }
}