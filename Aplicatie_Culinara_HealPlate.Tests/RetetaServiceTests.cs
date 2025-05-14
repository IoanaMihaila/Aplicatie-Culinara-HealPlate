using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;
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
                    Aprobata=false
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
                    Aprobata=false
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
    }
}
