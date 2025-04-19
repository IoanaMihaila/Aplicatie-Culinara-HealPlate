using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.AspNetCore.Mvc;

public class NotificariViewComponent : ViewComponent
{
    private readonly HealPlateDbContext _context;

    public NotificariViewComponent(HealPlateDbContext context)
    {
        _context = context;
    }

    public IViewComponentResult Invoke()
    {
        var userId = HttpContext.Session.GetInt32("IdUtilizator");

        if (userId != null)
        {
            var notificari = _context.Notificaris
                .Where(n => n.IdUtilizator == userId)
                .ToList();

            ViewData["NumarNotificariNevizualizate"] = notificari.Count(n => n.Vizualizat == false);

            return View(notificari);
        }

        ViewData["NumarNotificariNevizualizate"] = 0;
        return View(new List<Notificari>());
    }

}