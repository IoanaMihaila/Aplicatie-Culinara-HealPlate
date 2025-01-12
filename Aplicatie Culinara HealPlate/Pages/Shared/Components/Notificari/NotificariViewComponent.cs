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

        // Verifică dacă există un ID de utilizator în sesiune
        if (userId != null)
        {
            var notificari = _context.Notificaris
                .Where(n => n.IdAdmin == userId && n.Vizualizat == false)
            .ToList();

            return View(notificari);
        }

        return View(new List<Notificari>());
    }
}
