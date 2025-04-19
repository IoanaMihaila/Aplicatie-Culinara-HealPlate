using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aplicatie_Culinara_HealPlate.Pages.Shared.Components.Notificari
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificariController : Controller
    {
        private readonly HealPlateDbContext _context;

        public NotificariController(HealPlateDbContext context)
        {
            _context = context;
        }

        [HttpPost("MarcareVizualizat")]
        public async Task<IActionResult> MarcareVizualizat([FromBody] NotificareRequest request)
        {
            var notificare = await _context.Notificaris.FindAsync(request.NotificareId);

            if (notificare == null)
            {
                return Json(new { success = false, message = "Notificarea nu a fost găsită." });
            }

            notificare.Vizualizat = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost("StergeToate")]
        public async Task<IActionResult> StergeToateNotificarile()
        {
            var userId = HttpContext.Session.GetInt32("IdUtilizator");
            if (userId == null)
            {
                return Json(new { success = false, message = "Utilizatorul nu este autentificat." });
            }

            var notificari = _context.Notificaris.Where(n => n.IdUtilizator == userId);
            _context.Notificaris.RemoveRange(notificari);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
    public class NotificareRequest
    {
        public int NotificareId { get; set; }
    }
}
