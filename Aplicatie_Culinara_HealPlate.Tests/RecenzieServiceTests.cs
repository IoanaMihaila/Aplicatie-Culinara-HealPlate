using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Aplicatie_Culinara_HealPlate.Tests
{
    public class RecenzieServiceTests
    {
        private DbContextOptions<HealPlateDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<HealPlateDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddRecenzieAsync_HappyPath_ShouldAddRecenzie()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new RecenzieService(context);
                var recenzie = new Recenzii { IdUtilizator = 1, IdReteta = 2, TextRecenzie = "Foarte bună!", Scor = 5 };

                await service.AddRecenzieAsync(recenzie);

                Assert.Single(context.Recenziis);
            }
        }

        [Fact]
        public async Task DeleteRecenzieAsync_ErrorPath_ShouldNotThrow_WhenRecenzieNotFound()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new RecenzieService(context);

                var exception = await Record.ExceptionAsync(() => service.DeleteRecenzieAsync(999));

                Assert.Null(exception); // nu aruncă excepție dacă nu găsește recenzia
            }
        }

        [Fact]
        public void GetRecenzieByUtilizatorSiReteta_HappyPath_ShouldReturnCorrectRecenzie()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                context.Recenziis.Add(new Recenzii { IdRecenzie = 1, IdUtilizator = 10, IdReteta = 20, TextRecenzie = "Test", Scor = 4 });
                context.SaveChanges();

                var service = new RecenzieService(context);

                var recenzie = service.GetRecenzieByUtilizatorSiReteta(10, 20);

                Assert.NotNull(recenzie);
                Assert.Equal(4, recenzie.Scor);
            }
        }

        [Fact]
        public async Task UpdateRecenzieAsync_ErrorPath_ShouldNotThrow_WhenRecenzieNotFound()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new RecenzieService(context);

                var exception = await Record.ExceptionAsync(() => service.UpdateRecenzieAsync(1234, "Nou", 3));

                Assert.Null(exception); // dacă recenzia nu e găsită, metoda nu aruncă eroare
            }
        }
    }
}
