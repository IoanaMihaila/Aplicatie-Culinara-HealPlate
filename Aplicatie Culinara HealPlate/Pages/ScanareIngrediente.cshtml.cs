using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class ScanareIngredientModel : PageModel
    {
        private readonly VisionAPIService _visionApi;
        private readonly IRetetaService _retetaService;
        private readonly IConfiguration _config;

        public List<string> IngredienteRecunoscute { get; set; }
        public List<Retete> ReteteGasite { get; set; }
        public Dictionary<string, List<string>> AlergeniIngrediente { get; set; } = new();
        public string Mesaj { get; set; }

        public ScanareIngredientModel(IConfiguration config, IRetetaService retetaService)
        {
            _config = config;
            _retetaService = retetaService;
            _visionApi = new VisionAPIService(config["GoogleApiKey"]);
        }
        public async Task<IActionResult> OnPostAsync(IFormFile imagine)
        {
            if (imagine == null || imagine.Length == 0)
            {
                Mesaj = "Imagine invalidă.";
                return Page();
            }

            // Salvează imaginea pe disc în wwwroot/uploads/
            var fileName = Path.GetFileName(imagine.FileName);
            var uploadFolder = Path.Combine("wwwroot", "uploads");
            var filePath = Path.Combine(uploadFolder, fileName);

            Directory.CreateDirectory(uploadFolder); // asigură că folderul există

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imagine.CopyToAsync(fileStream);
            }

            // Setează calea pentru afișarea imaginii în .cshtml
            ViewData["UploadedImagePath"] = $"/uploads/{fileName}";

            // Citește fișierul și trimite la Vision API
            using var ms = new MemoryStream();
            await using (var readStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await readStream.CopyToAsync(ms);
            }

            var ingredienteEN = await _visionApi.DetectLabelsAsync(ms.ToArray());
            IngredienteRecunoscute = IngredientTranslator.Translate(ingredienteEN);

            if (!IngredienteRecunoscute.Any())
            {
                Mesaj = "Nu au fost detectate ingrediente cunoscute.";
                return Page();
            }

            ReteteGasite = _retetaService.CautaRetetePeBazaIngredientelor(IngredienteRecunoscute);

            foreach (var ing in IngredienteRecunoscute)
            {
                AlergeniIngrediente[ing] = _retetaService.GetAlergeniPentruIngredient(ing);
            }

            if (!ReteteGasite.Any())
            {
                Mesaj = "Nu s-au găsit rețete care conțin ingredientele detectate.";
            }

            return Page();
        }

    }
}
