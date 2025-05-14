using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;


namespace Aplicatie_Culinara_HealPlate.Pages
{
    public class PlanAlimentarModel : PageModel
    {
        private readonly HealPlateDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPlanAlimentarService _planService;

        public PlanAlimentarModel(HealPlateDbContext context, IEmailService emailService, IPlanAlimentarService planService)
        {
            _context = context;
            _emailService = emailService;
            _planService = planService;
        }
        public List<Retete> ReteteAlese { get; set; } = new List<Retete>();

        [HttpPost]
        public async Task<IActionResult> OnPostGenerarePlanAsync()
        {
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            if (!userId.HasValue)
                return RedirectToPage("/Autentificare");

            try
            {
                var retete = await _planService.GenereazaPlanAsync(userId.Value);

                if (retete == null || !retete.Any())
                    return new JsonResult(new { error = "Nu există rețete disponibile." });

                return new JsonResult(retete);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Eroare la generarea planului: {ex.Message}");
                return new JsonResult(new { error = "Eroare internă la generarea planului." });
            }
        }

        public async Task<IActionResult> OnPostSalvarePlanAsync([FromBody] JsonElement request)
        {
            if (!request.TryGetProperty("dataSelectata", out JsonElement dataSelectataElement) ||
                !request.TryGetProperty("retete", out JsonElement reteteElement) ||
                reteteElement.ValueKind != JsonValueKind.Array)
            {
                return BadRequest("Format JSON invalid sau lipsesc câmpuri.");
            }

            var dataSelectata = dataSelectataElement.GetString();
            if (string.IsNullOrWhiteSpace(dataSelectata))
                return BadRequest("Data este goală sau invalidă.");

            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            if (!userId.HasValue)
                return RedirectToPage("/Autentificare");

            var (success, message, plan, categorii) = await _planService.SalveazaPlanAsync(userId.Value, dataSelectata, reteteElement);

            if (!success)
                return new JsonResult(new { error = message });

            return new JsonResult(new
            {
                success = message,
                retete = categorii,
                ziua = plan.Ziua
            });
        }
        public async Task<IActionResult> OnGetPlanuriActiveAsync()
        {
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            if (!userId.HasValue)
                return RedirectToPage("/Autentificare");

            var planuri = await _planService.GetZilePlanificateAsync(userId.Value);
            return new JsonResult(planuri);
        }

    }
}
