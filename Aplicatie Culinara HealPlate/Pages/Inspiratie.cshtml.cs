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
        public async Task<IActionResult> OnPostGenerareRetetaAsync([FromBody] IngredientInputModel input)
        {
            if (input?.Ingrediente == null || !input.Ingrediente.Any())
                return BadRequest("Lista ingredientelor este goală.");

            var ingredienteRomanesti = input.Ingrediente
                .Select(i => TraducatorIngrediente.TraducereIngredient(i.ToLower()))
                .ToList();

            var reteta = GasesteReteta(ingredienteRomanesti);

            if (reteta == null)
                return new JsonResult(new { reteta = (object)null });

            return new JsonResult(new
            {
                reteta = new
                {
                    nume = reteta.Titlu,
                    descriere = reteta.Descriere
                }
            });
        }
        public class IngredientInputModel
        {
            public List<string> Ingrediente { get; set; }
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
        public async Task<IActionResult> OnPostSalveazaImagineAsync(IFormFile Imagine)
        {
            if (Imagine == null || Imagine.Length == 0)
                return BadRequest(new { error = "Imagine invalidă" });

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            Directory.CreateDirectory(uploadPath);

            var fileName = Path.GetRandomFileName() + Path.GetExtension(Imagine.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await Imagine.CopyToAsync(stream);

            var publicUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return new JsonResult(new { url = publicUrl });
        }

    }
}
