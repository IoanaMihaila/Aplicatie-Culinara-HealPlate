using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class ScanareIngredientModel : PageModel
    {
        private readonly VisionAPIService _visionApi;
        private readonly IRetetaService _retetaService;
        private readonly IConfiguration _config;

        public List<string> IngredienteRecunoscute { get; set; }
        public List<string> IngredienteRecunoscuteExtensie { get; set; }
        public List<Retete> ReteteGasite { get; set; }
        public Dictionary<string, List<string>> AlergeniIngrediente { get; set; } = new();
        public string Mesaj { get; set; }
        public List<List<string>> IstoricScanari { get; set; } = new();
        public bool RezultatDinExtensie { get; set; }


        public ScanareIngredientModel(IConfiguration config, IRetetaService retetaService)
        {
            _config = config;
            _retetaService = retetaService;
            _visionApi = new VisionAPIService(config);
        }

        public void OnGet(string ingrediente, string sursa)
        {
            if (!string.IsNullOrEmpty(ingrediente))
            {
                try
                {
                    var lista = JsonSerializer.Deserialize<List<string>>(ingrediente);
                    if (sursa == "extensie")
                    {
                        IngredienteRecunoscuteExtensie = lista;
                        ReteteGasite = _retetaService.CautaReteteCareContinCelPutinUnIngredient(lista);
                    }
                    else // fallback: imagine
                    {
                        IngredienteRecunoscute = lista;
                        ReteteGasite = _retetaService.CautaRetetePeBazaIngredientelor(lista);
                    }

                    AlergeniIngrediente = lista.ToDictionary(
                        ing => ing,
                        ing => _retetaService.GetAlergeniPentruIngredient(ing)
                    );
                }
                catch
                {
                    Mesaj = "Eroare la procesarea ingredientelor.";
                }
            }

            PreiaIstoricDinSesiune();
        }



        public async Task<IActionResult> OnPostAsync(IFormFile imagine)
        {
            if (imagine == null || imagine.Length == 0)
            {
                Mesaj = "Imagine invalidă.";
                return Page();
            }

            var fileName = Path.GetFileName(imagine.FileName);
            var uploadFolder = Path.Combine("wwwroot", "uploads");
            var filePath = Path.Combine(uploadFolder, fileName);

            Directory.CreateDirectory(uploadFolder);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imagine.CopyToAsync(fileStream);
            }

            ViewData["UploadedImagePath"] = $"/uploads/{fileName}";

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

            RezultatDinExtensie = false;
            IngredienteRecunoscuteExtensie = null;
            ActualizeazaIstoricInSesiune(IngredienteRecunoscute);
            PreiaIstoricDinSesiune();

            return Page();
        }

        public IActionResult OnPostRefoloseste(List<string> ingrediente)
        {
            IngredienteRecunoscute = ingrediente;
            ReteteGasite = _retetaService.CautaRetetePeBazaIngredientelor(ingrediente);
            AlergeniIngrediente = ingrediente.ToDictionary(
                ing => ing,
                ing => _retetaService.GetAlergeniPentruIngredient(ing)
            );

            PreiaIstoricDinSesiune();
            return Page();
        }

        public IActionResult OnPostStergeIstoric()
        {
            HttpContext.Session.Remove("IstoricScanari");
            IstoricScanari = new List<List<string>>();
            return Page();
        }

        private void ActualizeazaIstoricInSesiune(List<string> ingrediente)
        {
            const string sessionKey = "IstoricScanari";
            var istoric = HttpContext.Session.GetString(sessionKey);
            var lista = new List<List<string>>();

            if (!string.IsNullOrEmpty(istoric))
            {
                lista = JsonSerializer.Deserialize<List<List<string>>>(istoric);
            }

            lista.Insert(0, ingrediente);
            lista = lista.DistinctBy(l => string.Join(",", l)).Take(3).ToList();

            var json = JsonSerializer.Serialize(lista);
            HttpContext.Session.SetString(sessionKey, json);
        }

        private void PreiaIstoricDinSesiune()
        {
            const string sessionKey = "IstoricScanari";
            var istoric = HttpContext.Session.GetString(sessionKey);
            if (!string.IsNullOrEmpty(istoric))
            {
                IstoricScanari = JsonSerializer.Deserialize<List<List<string>>>(istoric);
            }
        }

        [EnableCors("PermiteExtensia")]
        [IgnoreAntiforgeryToken]
        public async Task<JsonResult> OnPostApiAsync([FromBody] List<string> ingrediente)
        {
            if (ingrediente == null || !ingrediente.Any())
            {
                return new JsonResult(new { mesaj = "Nu au fost primite ingrediente valide." }) { StatusCode = 400 };
            }

            var rezultat = new
            {
                ingrediente,
                alergeni = ingrediente.ToDictionary(i => i, i => _retetaService.GetAlergeniPentruIngredient(i)),
                retete = _retetaService.CautaRetetePeBazaIngredientelor(ingrediente)
            };

            return new JsonResult(rezultat);
        }

        public IActionResult OnPostCautaRetetePartial([FromForm] List<string> ingrediente)
        {
            IngredienteRecunoscuteExtensie = ingrediente;
            ReteteGasite = _retetaService.CautaReteteCareContinCelPutinUnIngredient(ingrediente);

            AlergeniIngrediente = ingrediente.ToDictionary(
                ing => ing,
                ing => _retetaService.GetAlergeniPentruIngredient(ing)
            );
            Console.WriteLine("Ingrediente primite: " + string.Join(", ", ingrediente));
            Console.WriteLine("Retete găsite: " + ReteteGasite.Count);

            PreiaIstoricDinSesiune();
            return Page();
        }

    }
}
