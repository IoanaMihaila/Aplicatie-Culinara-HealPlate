using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class InregistrareModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        private readonly IEmailService _emailService;
        private readonly PasswordHasher<Utilizatori> _passwordHasher;

        public InregistrareModel(HealPlateDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            _passwordHasher = new PasswordHasher<Utilizatori>();
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
            // Verifică dacă email-ul există deja în baza de date
            var existingUser = _context.Utilizatoris.FirstOrDefault(u => u.Email == Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email-ul introdus este deja folosit.");
                return Page();
            }

            // Crearea unui cod de verificare
            var verificationCode = EmailService.GenerateVerificationCode();

            // Salvează codul de verificare în sesiune pentru verificare ulterioară
            HttpContext.Session.SetString("VerificationCode", verificationCode);
            HttpContext.Session.SetString("PendingEmail", Email);
            HttpContext.Session.SetString("PendingUsername", Username);
            HttpContext.Session.SetString("PendingNume", Nume);
            HttpContext.Session.SetString("PendingPrenume", Prenume);
            HttpContext.Session.SetString("PendingParola", Parola);
            HttpContext.Session.SetString("PendingRestrictii", string.Join(",", Restrictii));

            // 3. Trimiterea emailului cu codul de verificare
            var subject = "Cod de verificare pentru HealPlate";
            var body = $"Bine ai venit in comunitatea noastra gastronomica sanatoasa! \nCodul tău de verificare este: {verificationCode}";
            await _emailService.SendEmailAsync(Email, subject, body);

            // 4. Afișează un mesaj pentru utilizator că trebuie să verifice emailul și să introducă codul
            TempData["Message"] = "Verifica-ti emailul pentru a valida inregistrarea in noul tau cont.";

            // Redirecționează utilizatorul la aceeași pagină de înregistrare
            return RedirectToPage("/Inregistrare");
        }


        public async Task<IActionResult> OnPostVerificaCodAsync([FromBody] VerificationRequest request)
        {
            var sessionVerificationCode = HttpContext.Session.GetString("VerificationCode");

            Console.WriteLine($"Codul de verificare din sesiune: {sessionVerificationCode}");
            Console.WriteLine($"Codul introdus: {request.VerificationCode}");

            if (sessionVerificationCode == request.VerificationCode)
            {
                // Dacă codul este corect, salvează utilizatorul în baza de date
                var utilizator = new Utilizatori
                {
                    Nume = HttpContext.Session.GetString("PendingNume"),
                    Prenume = HttpContext.Session.GetString("PendingPrenume"),
                    Email = HttpContext.Session.GetString("PendingEmail"),
                    Username = HttpContext.Session.GetString("PendingUsername"),
                    Parola = _passwordHasher.HashPassword(null, HttpContext.Session.GetString("PendingParola")), // Hash-uirea parolei
                };

                _context.Utilizatoris.Add(utilizator);
                await _context.SaveChangesAsync();

                var restrictii = HttpContext.Session.GetString("PendingRestrictii")?.Split(',').ToList();
                if (restrictii != null && restrictii.Any())
                {
                    foreach (var restrictie in restrictii)
                    {
                        Console.WriteLine($"Adăugarea alergenului: {restrictie}");
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

                // Poți adăuga un mesaj de succes sau redirecționezi utilizatorul
                return new JsonResult(new { success = true });
            }
            else
            {
                // Codul este incorect
                return new JsonResult(new { success = false, message = "Codul de verificare este incorect." });
            }
        }

        public class VerificationRequest
        {
            public string VerificationCode { get; set; }
        }
    }
}
