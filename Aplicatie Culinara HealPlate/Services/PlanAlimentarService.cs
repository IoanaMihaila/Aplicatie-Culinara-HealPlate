using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public interface IPlanAlimentarService
    {
        Task<List<Retete>> GenereazaPlanAsync(int userId);
        Task<(bool success, string message, PlanAlimentar plan, Dictionary<string, (int id, string titlu)> retete)> SalveazaPlanAsync(int userId, string dataSelectata, JsonElement reteteElement);
        Task<List<DateOnly>> GetZilePlanificateAsync(int userId);
    }

    public class PlanAlimentarService : IPlanAlimentarService
    {
        private readonly HealPlateDbContext _context;
        private readonly IEmailService _emailService;

        public PlanAlimentarService(HealPlateDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<Retete>> GenereazaPlanAsync(int userId)
        {
            var categorii = new List<string> { "Mic Dejun", "Prânz", "Cină", "Desert", "Gustare" };
            var reteteGenerate = new List<Retete>();

            var alergeniUtilizator = await _context.UtilizatorAlergenis
                .Where(au => au.IdUtilizator == userId)
                .Select(au => au.IdAlergen)
                .ToListAsync();

            var ingredienteCuAlergeni = await _context.IngredientAlergenis
                .Where(ia => alergeniUtilizator.Contains(ia.IdAlergen))
                .Select(ia => ia.IdIngredient)
                .ToListAsync();

            var toateRetetele = await _context.Retetes
                .Where(r => r.Aprobata == true && !_context.RetetaIngredientes
                    .Where(ri => ri.IdReteta == r.IdReteta)
                    .Select(ri => ri.IdIngredient)
                    .Any(idIngredient => ingredienteCuAlergeni.Contains(idIngredient)))
                .ToListAsync();

            foreach (var categorie in categorii)
            {
                var reteteCategorie = toateRetetele.Where(r => r.Categorie == categorie).ToList();
                if (reteteCategorie.Any())
                {
                    var retetaSelectata = reteteCategorie.OrderBy(r => Guid.NewGuid()).First();
                    reteteGenerate.Add(retetaSelectata);
                }
            }

            return reteteGenerate;
        }

        public async Task<(bool success, string message, PlanAlimentar plan, Dictionary<string, (int id, string titlu)> retete)> SalveazaPlanAsync(int userId, string dataSelectata, JsonElement reteteElement)
        {
            if (!DateOnly.TryParseExact(dataSelectata, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly ziua))
            {
                return (false, "Formatul datei este invalid.", null, null);
            }

            var utilizator = await _context.Utilizatoris.FirstOrDefaultAsync(u => u.IdUtilizator == userId);
            if (utilizator == null)
            {
                return (false, "Utilizatorul nu a fost găsit.", null, null);
            }

            var planExistent = await _context.PlanAlimentars
                .FirstOrDefaultAsync(p => p.IdUtilizator == userId && p.Ziua == ziua);
            if (planExistent != null)
            {
                return (false, "Planul alimentar pentru această zi a fost deja generat.", null, null);
            }

            var categorii = new Dictionary<string, (int Id, string Titlu)>();
            foreach (var retetaJson in reteteElement.EnumerateArray())
            {
                if (retetaJson.TryGetProperty("categorie", out var categorieEl) &&
                    retetaJson.TryGetProperty("idReteta", out var idEl) &&
                    retetaJson.TryGetProperty("titlu", out var titluEl) &&
                    idEl.TryGetInt32(out int idReteta))
                {
                    string categorie = categorieEl.GetString();
                    string titlu = titluEl.GetString();
                    if (!string.IsNullOrEmpty(categorie) && !string.IsNullOrEmpty(titlu))
                    {
                        categorii[categorie] = (idReteta, titlu);
                    }
                }
            }

            var planNou = new PlanAlimentar
            {
                IdUtilizator = userId,
                IdMicDeJun = categorii.ContainsKey("Mic Dejun") ? categorii["Mic Dejun"].Id : null,
                IdPranz = categorii.ContainsKey("Prânz") ? categorii["Prânz"].Id : null,
                IdCina = categorii.ContainsKey("Cină") ? categorii["Cină"].Id : null,
                IdDesert = categorii.ContainsKey("Desert") ? categorii["Desert"].Id : null,
                IdGustare = categorii.ContainsKey("Gustare") ? categorii["Gustare"].Id : null,
                Ziua = ziua
            };

            _context.PlanAlimentars.Add(planNou);
            int result = await _context.SaveChangesAsync();

            if (result == 0)
            {
                return (false, "Nicio modificare nu a fost salvată în baza de date.", null, null);
            }

            string mesajEmail = $@"
<h2>Plan alimentar generat</h2>
<p>Ai generat un plan alimentar pentru data de <strong>{ziua:yyyy-MM-dd}</strong>.</p>
<ul>
    {string.Join("", categorii.Select(r => $"<li>{r.Value.Titlu} - Categorie: {r.Key}</li>"))}
</ul>";

            await _emailService.SendEmailAsync(utilizator.Email, "Plan alimentar generat", mesajEmail);

            return (true, "Plan salvat cu succes!", planNou, categorii);
        }

        public async Task<List<DateOnly>> GetZilePlanificateAsync(int userId)
        {
            return await _context.PlanAlimentars
                .Where(p => p.IdUtilizator == userId)
                .Select(p => p.Ziua)
                .ToListAsync();
        }
    }
}

