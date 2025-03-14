using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aplicatie_Culinara_HealPlate.Pages.Shared.Components.Notificari
{
    public class NotificariController : Controller
    {
        private readonly Data.HealPlateDbContext _context;

        public NotificariController(Data.HealPlateDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult MarcareVizualizat(int notificareId)
        {
            var notificare = _context.Notificaris.Find(notificareId);
            if (notificare == null)
            {
                return new JsonResult(new { success = false, message = "Notificarea nu a fost găsită." });
            }

            notificare.Vizualizat = true;
            _context.SaveChanges();

            return new JsonResult(new { success = true });
        }
    }
}
