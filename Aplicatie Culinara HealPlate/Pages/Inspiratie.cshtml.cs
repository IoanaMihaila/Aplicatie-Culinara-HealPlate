using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Google.Cloud.Vision.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class InspiratieModel : PageModel
    {
        private readonly HealPlateDbContext _context;

        public InspiratieModel(HealPlateDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> OnPostGenerareRetetaAsync(IFormFile Imagine)
        {
            if (Imagine == null || Imagine.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Te rugăm să încarci o imagine validă.");
                return Page();
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, Imagine.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Imagine.CopyToAsync(stream);
            }
            ViewData["UploadedImagePath"] = $"/uploads/{Imagine.FileName}";

            // 🔍 Trimite imaginea la Google Vision API pentru recunoaștere
            List<string> ingredienteDetectate = await DetecteazaIngrediente(filePath);

            if (!ingredienteDetectate.Any())
            {
                return BadRequest("Nu s-au detectat ingrediente.");
            }

            // 🔎 Traducerea ingredientelor detectate în limba română
            List<string> ingredienteRomanesti = ingredienteDetectate
                .Select(ingredient => TraducatorIngrediente.TraducereIngredient(ingredient.ToLower()))
                .ToList();

            // 🔍 Logarea ingredientelor detectate și traduse în consola
            Console.WriteLine("Ingrediente detectate: ");
            foreach (var ingredient in ingredienteDetectate)
            {
                Console.WriteLine(ingredient);
            }

            Console.WriteLine("Ingrediente traduse: ");
            foreach (var ingredient in ingredienteRomanesti)
            {
                Console.WriteLine(ingredient);
            }

            // 🔎 Caută rețete care conțin ingredientele traduse
            var reteta = GasesteReteta(ingredienteRomanesti);

            if (reteta == null)
            {
                return NotFound("Nicio rețetă potrivită găsită.");
            }

            return new JsonResult(new { success = true, reteta });
        }

        // 🔍 Metodă pentru trimiterea imaginii la Google Vision API
        private async Task<List<string>> DetecteazaIngrediente(string imagePath)
        {
            List<string> ingrediente = new();

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\ioana\\OneDrive\\Desktop\\AC-info\\AN 3\\Licenta\\healplate-fe0050724052.json");

            try
            {
                var client = ImageAnnotatorClient.Create();
                var image = await Image.FromFileAsync(imagePath);
                var response = await client.DetectLabelsAsync(image);

                foreach (var label in response)
                {
                    if (label.Score > 0.7) // Filtrare după scor
                    {
                        ingrediente.Add(label.Description.ToLower());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare Google Vision: {ex.Message}");
            }

            return ingrediente;
        }

        // 🔎 Metodă pentru căutarea rețetelor în baza de date
        private Retete GasesteReteta(List<string> ingrediente)
        {
            return _context.Retetes
                .Where(r => r.RetetaIngredientes
                    .Any(ri => ingrediente.Contains(ri.IdIngredientNavigation.Nume.ToLower())))
                .OrderByDescending(r => r.RetetaIngredientes
                    .Count(ri => ingrediente.Contains(ri.IdIngredientNavigation.Nume.ToLower())))
                .FirstOrDefault();
        }
    }
}
