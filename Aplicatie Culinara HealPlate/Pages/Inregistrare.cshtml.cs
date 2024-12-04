using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Nume { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Prenumele este obligatoriu.")]
        public string Prenume { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email-ul este obligatoriu.")]
        [EmailAddress(ErrorMessage = "Introdu un email valid.")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Username-ul este obligatoriu.")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Parola este obligatorie.")]
        [MinLength(6, ErrorMessage = "Parola trebuie să aibă cel puțin 6 caractere.")]
        public string Parola { get; set; }

        [BindProperty]
        public List<string> Restrictii { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            {
                ModelState.AddModelError("Email", "Email-ul introdus nu este valid.");
            }
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Crearea unui utilizator nou
            var utilizator = new Utilizatori
            {
                Nume = Nume,
                Prenume = Prenume,
                Email = Email,
                Username = Username,
                Parola = Parola
            };

            // Adăugare utilizator în tabel
            _context.Utilizatoris.Add(utilizator);
            await _context.SaveChangesAsync();

            // 2. Adăugarea alergenilor selectați
            if (Restrictii.Any())
            {
                foreach (var restrictie in Restrictii)
                {
                    // Obține ID-ul alergenului după denumire (presupunem că tabelul Alergeni conține date preexistente)
                    var alergen = _context.Alergenis.FirstOrDefault(a => a.NumeAlergen == restrictie);
                    if (alergen != null)
                    {
                        var utilizatorAlergen = new UtilizatorAlergeni
                        {
                            IdUtilizator = utilizator.IdUtilizator,
                            IdAlergen = alergen.IdAlergen
                        };

                        _context.UtilizatorAlergenis.Add(utilizatorAlergen);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/VizualizareRetete");
        }
    }
}
