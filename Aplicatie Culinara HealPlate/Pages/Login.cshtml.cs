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
        public string Password { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validare: câmpurile sunt completate
            if (string.IsNullOrWhiteSpace(Credential) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Please enter both email/username and password.");
                return Page();
            }

            // Verificare utilizator în baza de date
            var user = await _context.Utilizatoris
                .FirstOrDefaultAsync(u => (u.Email == Credential || u.Username == Credential) && u.Parola == Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email/username or password.");
                return Page();
            }

            // Redirecționare în caz de succes
            return RedirectToPage("/Index");
        }
        public void OnGet()
        {
        }
    }
}
