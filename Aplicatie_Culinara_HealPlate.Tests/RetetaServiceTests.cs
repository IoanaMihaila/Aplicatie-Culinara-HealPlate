using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;
using Xunit;

namespace Aplicatie_Culinara_HealPlate.Tests
{
    public class RetetaServiceTests
    {
        private DbContextOptions<HealPlateDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<HealPlateDbContext>()
                .UseInMemoryDatabase(databaseName: "RetetaServiceTestDb_" + System.Guid.NewGuid())
                .Options;
        }

        [Fact]
        public void GetRetetaById_HappyPath_ShouldReturnReteta()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                context.Retetes.Add(new Retete
                {
                    IdReteta = 1,
                    Titlu = "Test",
                    Categorie = "Mic dejun",
                    Descriere = "Descriere de test",
                    Imagine = "poza.jpg",
                    ModDePreparare = "Se amestecă totul."
                });

                context.SaveChanges();

                var service = new RetetaService(context);

                var result = service.GetRetetaById(1);

                Assert.NotNull(result);
                Assert.Equal("Test", result.Titlu);
            }
        }

        [Fact]
        public void GetRetetaById_ErrorPath_ShouldReturnNull_WhenRetetaNotFound()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new RetetaService(context);
                var result = service.GetRetetaById(999); // id inexistent

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task ApprovePost1Async_HappyPath_ShouldApproveReteta()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                context.Retetes.Add(new Retete
                {
                    IdReteta = 1,
                    Titlu = "Test",
                    Categorie = "Mic dejun",
                    Descriere = "Descriere de test",
                    Imagine = "poza.jpg",
                    ModDePreparare = "Se amestecă totul.",
                    Aprobata = false
                });

                context.SaveChanges();

                var service = new RetetaService(context);
                var result = await service.ApprovePost1Async(1);

                Assert.True(result);
                Assert.True(context.Retetes.Find(1).Aprobata);
            }
        }

        [Fact]
        public async Task ApprovePost1Async_ErrorPath_ShouldReturnFalse_WhenRetetaNotFound()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new RetetaService(context);
                var result = await service.ApprovePost1Async(999);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task RejectPostAsync_HappyPath_ShouldRejectReteta()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                context.Retetes.Add(new Retete
                {
                    IdReteta = 1,
                    Titlu = "Test",
                    Categorie = "Mic dejun",
                    Descriere = "Descriere de test",
                    Imagine = "poza.jpg",
                    ModDePreparare = "Se amestecă totul.",
                    Aprobata = false
                });

                context.SaveChanges();

                var service = new RetetaService(context);
                var result = await service.RejectPostAsync(1);

                Assert.True(result);
                Assert.False(context.Retetes.Find(1).Aprobata);
            }
        }

        [Fact]
        public async Task RejectPostAsync_ErrorPath_ShouldReturnFalse_WhenRetetaNotFound()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new RetetaService(context);
                var result = await service.RejectPostAsync(999);

                Assert.False(result);
            }
        }
        [Fact]
        public async Task AddToCollectionAsync_HappyPath()
        {
            var options = GetInMemoryOptions();
            using var context = new HealPlateDbContext(options);
            context.Utilizatoris.Add(new Utilizatori { IdUtilizator = 1, Nume = "Test", Prenume = "Test", Email = "test@gmail.com", Parola = "Test", Rol = "Utilizator", Username = "Test" });
            await context.SaveChangesAsync();

            var service = new RetetaService(context);
            var result = await service.AddToCollectionAsync(1, 10);

            Assert.True(result.success);
            Assert.Equal("Rețeta a fost adăugată în colecție.", result.message);
        }

        [Fact]
        public async Task AddToCollectionAsync_Error_InvalidUser()
        {
            var options = GetInMemoryOptions();
            using var context = new HealPlateDbContext(options);
            var service = new RetetaService(context);

            var result = await service.AddToCollectionAsync(99, 10);

            Assert.False(result.success);
            Assert.Equal("Utilizatorul nu este autentificat.", result.message);
        }

        [Fact]
        public async Task RemoveFromCollectionAsync_HappyPath()
        {
            var options = GetInMemoryOptions();
            using var context = new HealPlateDbContext(options);
            var utilizator = new Utilizatori { IdUtilizator = 2, Nume = "Test", Prenume = "Test", Email = "test@gmail.com", Parola = "Test", Rol = "Utilizator", Username = "Test" };
            var colectie = new ColectiePersonala { IdUtilizator = 2, DataAdaugare = DateOnly.FromDateTime(DateTime.Now) };
            context.Utilizatoris.Add(utilizator);
            context.ColectiePersonalas.Add(colectie);
            await context.SaveChangesAsync();

            context.ColectiePersonalaRetetes.Add(new ColectiePersonalaRetete { IdColectie = colectie.IdColectie, IdReteta = 20 });
            await context.SaveChangesAsync();

            var service = new RetetaService(context);
            var result = await service.RemoveFromCollectionAsync(2, 20);

            Assert.True(result.success);
            Assert.Equal("Rețeta a fost ștearsă din colecție.", result.message);
        }

        [Fact]
        public async Task RemoveFromCollectionAsync_Error_NotFound()
        {
            var options = GetInMemoryOptions();
            using var context = new HealPlateDbContext(options);
            context.ColectiePersonalas.Add(new ColectiePersonala { IdUtilizator = 3 });
            await context.SaveChangesAsync();

            var service = new RetetaService(context);
            var result = await service.RemoveFromCollectionAsync(3, 99);

            Assert.False(result.success);
            Assert.Equal("Rețeta nu există în colecția ta.", result.message);
        }

        [Fact]
        public async Task DeleteRecipeAsync_HappyPath()
        {
            var options = GetInMemoryOptions();
            using var context = new HealPlateDbContext(options);
            context.Retetes.Add(new Retete { IdReteta = 100, Titlu = "Test", Categorie = "Test", Descriere = "Test", ModDePreparare = "Test", Imagine = "Test" });
            await context.SaveChangesAsync();

            var service = new RetetaService(context);
            var result = await service.DeleteRecipeAsync(100);

            Assert.True(result.success);
            Assert.Equal("Rețeta a fost ștearsă cu succes.", result.message);
        }

        [Fact]
        public async Task DeleteRecipeAsync_Error_NotFound()
        {
            var options = GetInMemoryOptions();
            using var context = new HealPlateDbContext(options);
            var service = new RetetaService(context);

            var result = await service.DeleteRecipeAsync(999);

            Assert.False(result.success);
            Assert.Equal("Rețeta nu a fost găsită.", result.message);
        }
    }
}
