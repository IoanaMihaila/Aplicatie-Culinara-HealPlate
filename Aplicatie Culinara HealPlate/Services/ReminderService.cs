using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;

public class ReminderService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmailService _emailService;

    public ReminderService(IServiceProvider serviceProvider, IEmailService emailService)
    {
        _serviceProvider = serviceProvider;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<HealPlateDbContext>();

                var dataAstazi = DateOnly.FromDateTime(DateTime.Today);
                var planuriAstazi = await _context.PlanAlimentars
                    .Where(p => p.Ziua == dataAstazi)
                    .ToListAsync();


                foreach (var plan in planuriAstazi)
                {
                    var utilizator = await _context.Utilizatoris
                        .Where(u => u.IdUtilizator == plan.IdUtilizator)
                        .FirstOrDefaultAsync();

                    if (utilizator == null) continue; 

                    var retete = new List<string>();
                    if (plan.IdMicDeJun.HasValue)
                        retete.Add(await _context.Retetes.Where(r => r.IdReteta == plan.IdMicDeJun).Select(r => $"{r.Titlu} (Mic Dejun)").FirstOrDefaultAsync());
                    if (plan.IdPranz.HasValue)
                        retete.Add(await _context.Retetes.Where(r => r.IdReteta == plan.IdPranz).Select(r => $"{r.Titlu} (Prânz)").FirstOrDefaultAsync());
                    if (plan.IdCina.HasValue)
                        retete.Add(await _context.Retetes.Where(r => r.IdReteta == plan.IdCina).Select(r => $"{r.Titlu} (Cină)").FirstOrDefaultAsync());
                    if (plan.IdDesert.HasValue)
                        retete.Add(await _context.Retetes.Where(r => r.IdReteta == plan.IdDesert).Select(r => $"{r.Titlu} (Desert)").FirstOrDefaultAsync());
                    if (plan.IdGustare.HasValue)
                        retete.Add(await _context.Retetes.Where(r => r.IdReteta == plan.IdGustare).Select(r => $"{r.Titlu} (Gustare)").FirstOrDefaultAsync());

                    var mesajEmail = $@"
                        <h2>Reminder Plan Alimentar</h2>
                        <p>Astăzi este ziua planificată pentru planul alimentar generat.</p>
                        <p>Rețetele tale de astăzi:</p>
                        <ul>
                            {string.Join("", retete.Select(r => $"<li>{r}</li>"))}
                        </ul>
                        <p>Spor la gătit!</p>
                     ";

                    await _emailService.SendEmailAsync(utilizator.Email, "Reminder: Plan alimentar de astăzi", mesajEmail);
                }


                // Așteaptă 24 de ore înainte de următoarea verificare
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}