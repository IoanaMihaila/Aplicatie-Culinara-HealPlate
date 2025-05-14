
using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using Xunit;

namespace Aplicatie_Culinara_HealPlate.Tests.Services
{
    public class PlanAlimentarServiceTests
    {
        private HealPlateDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<HealPlateDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new HealPlateDbContext(options);
            return context;
        }

        [Fact]
        public async Task GenereazaPlanAsync_HappyPath_ShouldReturnRecipes()
        {
            var context = GetInMemoryDbContext();

            context.Retetes.Add(new Retete { IdReteta = 1, Titlu = "Omleta", Categorie = "Mic Dejun", Aprobata = true, Descriere="test", Imagine="test.jpg", ModDePreparare="test test" });
            context.SaveChanges();

            var mockEmailService = new Mock<IEmailService>();
            var service = new PlanAlimentarService(context, mockEmailService.Object);

            var result = await service.GenereazaPlanAsync(1);

            Assert.NotNull(result);
            Assert.True(result.Any());
        }

        [Fact]
        public async Task SalveazaPlanAsync_InvalidDate_ShouldReturnError()
        {
            var context = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            var service = new PlanAlimentarService(context, mockEmailService.Object);

            var reteteJson = JsonDocument.Parse("[]").RootElement;
            var result = await service.SalveazaPlanAsync(1, "invalid-date", reteteJson);

            Assert.False(result.success);
            Assert.Equal("Formatul datei este invalid.", result.message);
        }

        [Fact]
        public async Task SalveazaPlanAsync_UserNotFound_ShouldReturnError()
        {
            var context = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            var service = new PlanAlimentarService(context, mockEmailService.Object);

            var json = @"[
                {""categorie"":""Mic Dejun"",""idReteta"":1,""titlu"":""Omleta""}
            ]";
            var element = JsonDocument.Parse(json).RootElement;
            var result = await service.SalveazaPlanAsync(99, "2025-05-14", element);

            Assert.False(result.success);
            Assert.Equal("Utilizatorul nu a fost găsit.", result.message);
        }

        [Fact]
        public async Task SalveazaPlanAsync_HappyPath_ShouldSavePlan()
        {
            var context = GetInMemoryDbContext();
            var mockEmailService = new Mock<IEmailService>();
            mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .Returns(Task.CompletedTask);

            var user = new Utilizatori { IdUtilizator = 1, Email = "test@example.com", Nume="Test", Parola="parola", Prenume="Test", Rol="Utilizator", Username="username" };
            context.Utilizatoris.Add(user);
            context.SaveChanges();

            var service = new PlanAlimentarService(context, mockEmailService.Object);

            var json = @"[
                {""categorie"":""Mic Dejun"",""idReteta"":1,""titlu"":""Omleta""}
            ]";
            var element = JsonDocument.Parse(json).RootElement;
            var result = await service.SalveazaPlanAsync(1, "2025-05-15", element);

            Assert.True(result.success);
            Assert.NotNull(result.plan);
            Assert.Equal("Plan salvat cu succes!", result.message);
        }
    }
}
