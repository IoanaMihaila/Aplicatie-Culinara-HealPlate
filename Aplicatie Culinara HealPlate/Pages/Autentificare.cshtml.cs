using Aplicatie_Culinara_HealPlate.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class LoginModel : PageModel
    {
        private readonly HealPlateDbContext _context;

        public LoginModel(HealPlateDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Credential { get; set; }

        [BindProperty]
        public string Parola { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Clear();
            // Validare: câmpurile sunt completate
            if (string.IsNullOrWhiteSpace(Credential) || string.IsNullOrWhiteSpace(Parola))
            {
                ModelState.AddModelError(string.Empty, "Te rog introdu atat email-ul/username-ul, cat si parola.");
                return Page();
            }

            // Verificare utilizator în baza de date
            var user = await _context.Utilizatoris
                .FirstOrDefaultAsync(u => (u.Email == Credential || u.Username == Credential) && u.Parola == Parola);

            if (user == null)
            {
                ModelState.AddModelError("Credential", "Email/username sau parola incorecta.");
                return Page();
            }

            // Redirecționare în caz de succes
            return RedirectToPage("/VizualizareRetete");
        }
        public void OnGet()
        {
        }
    }
}
