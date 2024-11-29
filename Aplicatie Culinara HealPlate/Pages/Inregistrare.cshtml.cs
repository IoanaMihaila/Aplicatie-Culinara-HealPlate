using Aplicatie_Culinara_HealPlate.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class InregistrareModel : PageModel
    {
        private readonly HealPlateDbContext _context;

        public InregistrareModel(HealPlateDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Nume { get; set; }

        [BindProperty]
        public string Prenume { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Parola { get; set; }
        public void OnGet()
        {
        }
    }
}
