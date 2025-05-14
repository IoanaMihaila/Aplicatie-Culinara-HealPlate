using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;
using Aplicatie_Culinara_HealPlate.Data;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class VizualizareDetaliiModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IRetetaService _retetaService;
        private readonly HealPlateDbContext _context;
        private readonly IRecenzieService _recenzieService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICosService _cosService;

        public VizualizareDetaliiModel(IRetetaService retetaService, HealPlateDbContext context, IRecenzieService recenzieService, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, ICosService cosService)
        {
            _retetaService = retetaService;
            _context = context;
            _recenzieService = recenzieService;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _cosService = cosService;
        }

        public Retete Reteta { get; set; }
        // Model pentru recenzia introdusă
        [BindProperty]
        public string RecenzieText { get; set; }
        [BindProperty]
        public int Scor { get; set; }
        public string ErrorMessage { get; set; }
        public IActionResult OnGet(int id)
        {
            // Căutăm rețeta după ID-ul primit
            Reteta = _retetaService.GetRetetaById(id);

            // Dacă rețeta nu există, returnăm o eroare 404
            if (Reteta == null)
            {
                return NotFound();
            }
            // Verificăm dacă utilizatorul este autentificat
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare"); // Redirect către pagina de login dacă utilizatorul nu este autentificat
            }

            // Verificăm dacă utilizatorul a lăsat deja o recenzie pentru această rețetă
            var utilizator = _context.Utilizatoris.FirstOrDefault(u => u.IdUtilizator == idUtilizator);
            if (utilizator != null)
            {
                var recenzieExistenta = _recenzieService.GetRecenzieByUtilizatorSiReteta(utilizator.IdUtilizator, id);
                if (recenzieExistenta != null)
                {
                    ErrorMessage = "Ai deja o recenzie pentru această rețetă!";
                }
            }


            // Încarcă recenziile cu utilizatorii asociați
            Reteta.Recenziis = _context.Recenziis
                .Include(r => r.IdUtilizatorNavigation)  // Include utilizatorul asociat
                .Where(r => r.IdReteta == id)  // Filtrare după rețeta respectivă
                .ToList();
            // Creăm un set pentru alergeni pentru a evita duplicatele
            var alergeniReteta = new HashSet<string>();

            foreach (var ingredient in Reteta.RetetaIngredientes)
            {
                foreach (var alergen in ingredient.IdIngredientNavigation.IngredientAlergenis)
                {
                    if (alergen.IdAlergenNavigation != null)
                    {
                        alergeniReteta.Add(alergen.IdAlergenNavigation.NumeAlergen);
                    }
                }
            }
            // Setăm alergeni pentru rețetă, care vor fi folosiți în cshtml
            ViewData["AlergeniReteta"] = alergeniReteta.Any() ? string.Join(", ", alergeniReteta) : "Nu există alergeni";
            // Returnăm pagina cu detaliile rețetei
            return Page();
        }

        // Metoda POST care redirecționează către aceeași pagină
        public IActionResult OnPost(int id)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare"); // Redirect la pagina de login dacă utilizatorul nu este autentificat
            }
            var utilizator = _context.Utilizatoris.FirstOrDefault(u => u.IdUtilizator == idUtilizator.Value);

            // Adăugăm recenzia în baza de date
            var recenzie = new Recenzii
            {
                IdReteta = id,
                TextRecenzie = RecenzieText,
                Scor = Scor,
                IdUtilizator = utilizator.IdUtilizator
            };
            if (utilizator == null)
            {
                return RedirectToPage("/Autentificare");
            }
            if (string.IsNullOrEmpty(RecenzieText) || Scor < 1 || Scor > 5)
            {
                ErrorMessage = "Recenzia trebuie să aibă un text valid și un scor între 1 și 5.";
                return RedirectToPage("./VizualizareDetalii", new { id });
            }
            _recenzieService.AddRecenzieAsync(recenzie);
            // Redirecționăm către aceleași detalii ale rețetei
            return RedirectToPage("./VizualizareDetalii", new { id = id });
        }
        public async Task<IActionResult> OnPostStergereRecenzieAsync(int id, int idReteta)
        {
            Console.WriteLine("Am ajuns in metoda OnPostStergereRecenzie");

            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");
            var rol = HttpContext.Session.GetString("Rol");

            if (!idUtilizator.HasValue || string.IsNullOrEmpty(rol))
            {
                return RedirectToPage("/Autentificare");
            }

            Recenzii? recenzie = null;

            if (rol == "Admin")
            {
                recenzie = await _context.Recenziis
                    .FirstOrDefaultAsync(r => r.IdRecenzie == id);
            }
            else if (rol == "Utilizator")
            {
                recenzie = await _context.Recenziis
                    .FirstOrDefaultAsync(r => r.IdRecenzie == id && r.IdUtilizator == idUtilizator.Value);
            }

            if (recenzie == null)
            {
                return NotFound();
            }

            _context.Recenziis.Remove(recenzie);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la salvarea modificărilor: {ex.Message}");
                return StatusCode(500, "A apărut o eroare la salvarea modificărilor.");
            }

            // Redirect către pagina detaliată a rețetei
            return RedirectToPage("./VizualizareDetalii", new { id = idReteta });
        }

        public async Task<IActionResult> OnPostEditareRecenzieAsync(int id, string textNou, int scorNou)
        {
            var idUtilizator = HttpContext.Session.GetInt32("IdUtilizator");

            if (!idUtilizator.HasValue)
            {
                return RedirectToPage("/Autentificare");
            }

            var recenzie = _context.Recenziis
                .FirstOrDefault(r => r.IdRecenzie == id && r.IdUtilizator == idUtilizator.Value);

            if (recenzie == null)
            {
                return NotFound();
            }

            recenzie.TextRecenzie = textNou;
            recenzie.Scor = scorNou;

            await _context.SaveChangesAsync();

            return RedirectToPage("./VizualizareDetalii", new { id = recenzie.IdReteta });
        }
        public async Task<IActionResult> OnPostAdaugaInCosAsync([FromBody] AdaugaInCosRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Cererea este invalidă." });
            }

            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            if (!userId.HasValue)
            {
                return BadRequest(new { success = false, message = "Utilizatorul nu este autentificat." });
            }

            var (success, message) = await _cosService.AdaugaIngredientInCosAsync(userId.Value, request);

            return new JsonResult(new { success, message });
        }

        public async Task<IActionResult> OnPostApprovePostAsync([FromBody] PostApproveRequest request)
        {
            Console.WriteLine("Am intrat în metoda OnPostApprovePostAsync"); // Log inițial
            if (request == null)
            {
                Console.WriteLine("Request-ul primit este null.");
                return new JsonResult(new { success = false, message = "Datele trimise sunt invalide." });
            }

            Console.WriteLine($"ID rețetă primit: {request.IdReteta}");

            try
            {
                var success = await _retetaService.ApprovePost1Async(request.IdReteta);

                if (success)
                {
                    // Obținem rețeta aprobată
                    var reteta = await _context.Retetes
                        .FirstOrDefaultAsync(r => r.IdReteta == request.IdReteta);

                    if (reteta != null)
                    {
                        // Extragem utilizatorul care a adăugat rețeta
                        var colectiePersonalaReteta = await _context.ColectiePersonalaRetetes
                            .Include(cpr => cpr.IdColectieNavigation) // Include navigarea către ColectiePersonala
                            .FirstOrDefaultAsync(cpr => cpr.IdReteta == reteta.IdReteta);
                        if (colectiePersonalaReteta?.IdColectieNavigation == null)
                        {
                            Console.WriteLine("Navigarea către ColectiePersonala nu a fost găsită.");
                            return new JsonResult(new { success = false, message = "Datele asociate rețetei nu au fost găsite." });
                        }
                        if (colectiePersonalaReteta != null)
                        {
                            var utilizatorId = colectiePersonalaReteta.IdColectieNavigation.IdUtilizator;

                            // Creăm notificarea pentru utilizator
                            var notificareUtilizator = new Notificari
                            {
                                Mesaj = $"Adminul a aprobat rețeta ta: {reteta.Titlu}.",
                                DataCreare = DateTime.Now,
                                IdUtilizator = utilizatorId,
                                IdReteta = reteta.IdReteta,
                                Vizualizat = false
                            };

                            _context.Notificaris.Add(notificareUtilizator);
                            await _context.SaveChangesAsync(); // Salvăm notificarea în baza de date
                        }
                    }

                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Rețeta nu a putut fi aprobată." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în procesarea cererii: {ex.Message}");
                return new JsonResult(new { success = false, message = "A apărut o eroare internă." });
            }
        }
        public async Task<IActionResult> OnPostRejectPostAsync([FromBody] PostRejectRequest request)
        {
            Console.WriteLine("Am intrat în metoda OnPostRejectPostAsync");

            if (request == null || request.IdReteta <= 0)
            {
                Console.WriteLine("Request-ul primit este invalid.");
                return new JsonResult(new { success = false, message = "Datele trimise sunt invalide." });
            }

            Console.WriteLine($"ID rețetă primit pentru respingere: {request.IdReteta}");

            try
            {
                var success = await _retetaService.RejectPostAsync(request.IdReteta);

                if (success)
                {
                    var reteta = await _context.Retetes
                        .FirstOrDefaultAsync(r => r.IdReteta == request.IdReteta);

                    if (reteta != null)
                    {
                        var colectiePersonalaReteta = await _context.ColectiePersonalaRetetes
                            .Include(cpr => cpr.IdColectieNavigation)
                            .FirstOrDefaultAsync(cpr => cpr.IdReteta == reteta.IdReteta);

                        if (colectiePersonalaReteta?.IdColectieNavigation == null)
                        {
                            Console.WriteLine("Navigarea către ColectiePersonala nu a fost găsită.");
                            return new JsonResult(new { success = false, message = "Datele asociate rețetei nu au fost găsite." });
                        }

                        var utilizatorId = colectiePersonalaReteta.IdColectieNavigation.IdUtilizator;

                        var notificareUtilizator = new Notificari
                        {
                            Mesaj = $"Adminul a respins rețeta ta: {reteta.Titlu}.",
                            DataCreare = DateTime.Now,
                            IdUtilizator = utilizatorId,
                            IdReteta = reteta.IdReteta,
                            Vizualizat = false
                        };

                        _context.Notificaris.Add(notificareUtilizator);
                        await _context.SaveChangesAsync();
                    }

                    return new JsonResult(new { success = true });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Rețeta nu a putut fi respinsă." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare în procesarea cererii: {ex.Message}");
                return new JsonResult(new { success = false, message = "A apărut o eroare internă." });
            }
        }

        public async Task<IActionResult> OnPostUpdateIngredient([FromBody] IngredientUpdateModel model)
        {
            if (model == null || model.IdReteta <= 0 || model.IdIngredient <= 0 || string.IsNullOrWhiteSpace(model.NumeIngredientNou) || model.CantitateNoua <= 0)
            {
                return new JsonResult(new { success = false, message = "Date invalide." });
            }

            try
            {
                // 1️⃣ Verificăm dacă ingredientul NOU există deja în tabela Ingrediente
                var existingIngredient = await _context.Ingredientes
                    .FirstOrDefaultAsync(i => i.Nume == model.NumeIngredientNou);

                int newIngredientId;

                if (existingIngredient == null)
                {
                    // 2️⃣ Dacă noul ingredient NU există, îl adăugăm
                    var newIngredient = new Ingrediente
                    {
                        Nume = model.NumeIngredientNou
                    };

                    _context.Ingredientes.Add(newIngredient);
                    await _context.SaveChangesAsync();
                    newIngredientId = newIngredient.IdIngredient;
                }
                else
                {
                    // Dacă există, folosim ID-ul lui
                    newIngredientId = existingIngredient.IdIngredient;
                }

                // 3️⃣ Găsim ingredientul asociat rețetei în Reteta_Ingrediente
                var retetaIngredient = await _context.RetetaIngredientes
                    .FirstOrDefaultAsync(ri => ri.IdReteta == model.IdReteta && ri.IdIngredient == model.IdIngredient);

                if (retetaIngredient == null)
                {
                    return new JsonResult(new { success = false, message = "Ingredientul nu este asociat cu această rețetă." });
                }

                // 4️⃣ Actualizăm înregistrarea din Reteta_Ingrediente
                retetaIngredient.IdIngredient = newIngredientId;
                retetaIngredient.Cantitate = model.CantitateNoua;
                retetaIngredient.Unitate = model.UnitateNoua;

                await _context.SaveChangesAsync();

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Eroare la actualizare: " + ex.Message });
            }
        }
        public async Task<IActionResult> OnPostUpdatePreparation([FromBody] UpdatePreparationModel model)
        {
            if (model == null || model.IdReteta <= 0 || string.IsNullOrWhiteSpace(model.ModDePreparareNou))
            {
                return new JsonResult(new { success = false, message = "Date invalide!" });
            }

            var reteta = await _context.Retetes.FindAsync(model.IdReteta);
            if (reteta == null)
            {
                return new JsonResult(new { success = false, message = "Rețeta nu a fost găsită!" });
            }

            reteta.ModDePreparare = model.ModDePreparareNou;
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = true });
        }
    }
    public class UpdatePreparationModel
    {
        public int IdReteta { get; set; }
        public string ModDePreparareNou { get; set; }
    }
    public class IngredientUpdateModel
    {
        public int IdReteta { get; set; }  // Adăugat ID-ul rețetei pentru siguranță
        public int IdIngredient { get; set; }
        public string NumeIngredientNou { get; set; }
        public decimal CantitateNoua { get; set; }
        public string UnitateNoua { get; set; }
    }


    // Clasa pentru Request Body
    public class PostApproveRequest
    {
        public int IdReteta { get; set; }
    }
    public class PostRejectRequest
    {
        public int IdReteta { get; set; }
    }
    public class AdaugaInCosRequest
    {
        public int IdIngredient { get; set; }
        public double Cantitate { get; set; }
        public string Unitate { get; set; }
    }
}

