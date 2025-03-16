using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class ColectiaPersonalaModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        public ColectiaPersonalaModel(HealPlateDbContext context)
        {
            _context = context;
        }
        [BindProperty]
        public List<Retete> Retete { get; set; } = new List<Retete>();
        // Proprietați pentru formular
        [BindProperty]
        public string Titlu { get; set; }
        [BindProperty]
        public IFormFile Imagine { get; set; }
        [BindProperty]
        public string Categorie { get; set; }
        [BindProperty]
        public List<string> Ingredient { get; set; }
        [BindProperty]
        public List<decimal> Cantitate { get; set; }
        [BindProperty]
        public List<string> Unitate { get; set; }
        [BindProperty]
        public string Descriere { get; set; }
        [BindProperty]
        public string ModDePreparare { get; set; }
        [BindProperty]
        public int TimpPreparare { get; set; }
        public Dictionary<int, bool> EsteInColectie { get; set; } = new();
        public List<string> Categorii { get; set; } = new List<string> { "Toate", "Mic dejun", "Prânz", "Cină", "Desert", "Gustare" };
        public string CategorieSelectata { get; set; } = "Toate";
        public void OnGet(string? categorie = null)
        {
            CategorieSelectata = categorie ?? "Toate";

            // Obține ID-ul utilizatorului curent din sesiune
            var userId = HttpContext.Session.GetInt32("IdUtilizator");

            // Verificăm dacă utilizatorul este conectat
            if (userId == null)
            {
                // Dacă nu există un utilizator conectat, nu afișăm nimic
                Retete = new List<Retete>();
                return;
            }

            // Obține rețetele din baza de date
            IQueryable<Retete> query = _context.Retetes;

            // Filtrare pe categorie (dacă este specificată)
            if (!string.IsNullOrEmpty(categorie) && categorie != "Toate")
            {
                query = query.Where(r => r.Categorie == categorie);
            }

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

                // Filtrăm doar rețetele care sunt în colecția personală a utilizatorului
                Retete = query.Where(r => reteteInColectie.Contains(r.IdReteta)).ToList();
            }
            else
            {
                // Dacă utilizatorul nu are o colecție personală, nu se va adăuga nimic în Retete
                Retete = new List<Retete>();
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("IdUtilizator");
                var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);

                // 1. Procesarea imaginii (dacă există)
                string imaginePath = null;
                if (Imagine != null && Imagine.Length > 0)
                {
                    // Creăm un nume unic pentru fișierul imaginii
                    var fileName = Path.GetFileName(Imagine.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    // Salvăm fișierul pe server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Imagine.CopyToAsync(stream);
                    }

                    // Salvează calea relativă a imaginii în baza de date
                    imaginePath = "/images/" + fileName;
                }

                // 2. Salvarea rețetei în tabela Retete
                var reteta = new Retete
                {
                    Titlu = Titlu,
                    Categorie = Categorie,
                    Descriere = Descriere,
                    ModDePreparare = ModDePreparare,
                    TimpPreparare = TimpPreparare,
                    Imagine = imaginePath // Atribuim calea imaginii
                };
                reteta.Aprobata = null;
                _context.Retetes.Add(reteta);
                await _context.SaveChangesAsync();

                // 3. Salvarea ingredientelor în tabela Ingrediente (dacă nu există deja)
                for (int i = 0; i < Ingredient.Count; i++)
                {
                    var ingredient = _context.Ingredientes.FirstOrDefault(ing => ing.Nume == Ingredient[i]);

                    if (ingredient == null)
                    {
                        ingredient = new Ingrediente { Nume = Ingredient[i] };
                        _context.Ingredientes.Add(ingredient);
                        await _context.SaveChangesAsync();
                    }

                    var retetaIngrediente = new RetetaIngrediente
                    {
                        IdReteta = reteta.IdReteta,
                        IdIngredient = ingredient.IdIngredient,
                        Cantitate = Cantitate[i],
                        Unitate = Unitate[i]
                    };

                    _context.RetetaIngredientes.Add(retetaIngrediente);
                }

                // 4. Verificăm dacă utilizatorul are deja o colecție personală
                var colectie = await _context.ColectiePersonalas
                    .FirstOrDefaultAsync(c => c.IdUtilizator == utilizator.IdUtilizator);

                if (colectie == null)
                {
                    colectie = new ColectiePersonala
                    {
                        IdUtilizator = utilizator.IdUtilizator,
                        DataAdaugare = DateOnly.FromDateTime(DateTime.Now)
                    };

                    _context.ColectiePersonalas.Add(colectie);
                    await _context.SaveChangesAsync();
                }

                // 5. Adăugăm legătura între ColectiaPersonala și Retete
                var colectiePersonalaReteta = new ColectiePersonalaRetete
                {
                    IdColectie = colectie.IdColectie,
                    IdReteta = reteta.IdReteta
                };
                _context.ColectiePersonalaRetetes.Add(colectiePersonalaReteta);

                // 6. Obținem ID-ul adminului din baza de date
                var admin = await _context.Utilizatoris
                    .FirstOrDefaultAsync(u => u.Rol == "Admin"); // Verificăm dacă există un utilizator cu rolul 'Admin'

                // 7. Dacă există un admin, creăm notificarea
                if (admin != null)
                {
                    var notificareAdmin = new Notificari
                    {
                        Mesaj = $"Utilizatorul {utilizator.Nume} a adaugat o reteta noua care asteapta aprobare.",
                        DataCreare = DateTime.Now, // Setăm data curentă ca fiind data creării notificării
                        IdUtilizator = admin.IdUtilizator, // Setăm ID-ul adminului
                        IdReteta = reteta.IdReteta, // Setăm ID-ul rețetei adăugate
                        Vizualizat = null // Presupunem că notificarea nu a fost vizualizată inițial
                    };

                    _context.Notificaris.Add(notificareAdmin);
                    await _context.SaveChangesAsync(); // Salvăm notificarea în baza de date
                }
                else
                {
                    // Dacă nu există admin, logăm un mesaj de eroare sau opțional notificăm utilizatorul
                    // Logăm eroarea
                    Console.WriteLine("Nu există un administrator în baza de date.");
                }

                // 8. Setează notificarea pentru utilizator (mesajul va fi afișat pe următoarea pagină)
                TempData["Notificare"] = "Reteta a fost adaugata cu succes! Asteapta aprobare.";

                return RedirectToPage(); // Răspunsul la submit este tot pe aceeași pagină
            }
            return Page(); // Dacă există erori, revenim pe pagina curentă
        }

    }
}
