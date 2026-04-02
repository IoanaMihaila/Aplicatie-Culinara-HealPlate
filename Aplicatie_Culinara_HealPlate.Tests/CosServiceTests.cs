using Aplicatie_Culinara_HealPlate.Data;
using Aplicatie_Culinara_HealPlate.Models;
using Aplicatie_Culinara_HealPlate.Pages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using System;
using Aplicatie_Culinara_HealPlate.Services;

namespace Aplicatie_Culinara_HealPlate.Tests
{
    public class CosServiceTests
    {
        private DbContextOptions<HealPlateDbContext> GetInMemoryOptions()
        {
            return new DbContextOptionsBuilder<HealPlateDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AdaugaIngredientInCosAsync_HappyPath_ShouldAddIngredient()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                context.Utilizatoris.Add(new Utilizatori
                {
                    IdUtilizator = 1,
                    Email = "test@test.com",
                    Nume = "Popescu",
                    Prenume = "Ion",
                    Parola = "1234",
                    Rol = "Utilizator",
                    Username = "ion123"
                });

                context.SaveChanges();

                var service = new CosService(context);

                var request = new AdaugaInCosRequest
                {
                    IdIngredient = 10,
                    Cantitate = 150,
                    Unitate = "g"
                };

                var result = await service.AdaugaIngredientInCosAsync(1, request);

                Assert.True(result.success);
                Assert.Equal("Ingredient adăugat în coș!", result.message);
            }
        }

        [Fact]
        public async Task AdaugaIngredientInCosAsync_ErrorPath_UserNotFound()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var service = new CosService(context);

                var request = new AdaugaInCosRequest
                {
                    IdIngredient = 1,
                    Cantitate = 50,
                    Unitate = "ml"
                };

                var result = await service.AdaugaIngredientInCosAsync(999, request);

                Assert.False(result.success);
                Assert.Equal("Utilizatorul nu a fost găsit.", result.message);
            }
        }

        [Fact]
        public async Task AdaugaIngredientInCosAsync_ErrorPath_IngredientAlreadyExists()
        {
            var options = GetInMemoryOptions();

            using (var context = new HealPlateDbContext(options))
            {
                var user = new Utilizatori { IdUtilizator = 2 };
                context.Utilizatoris.Add(new Utilizatori
                {
                    IdUtilizator = 2,
                    Email = "test@test.com",
                    Nume = "Popescu",
                    Prenume = "Ion",
                    Parola = "1234",
                    Rol = "Utilizator",
                    Username = "ion123"
                });

                context.SaveChanges();

                var cos = new CosuriDeCumparaturi
                {
                    IdUtilizator = user.IdUtilizator,
                    DataCreare = DateOnly.FromDateTime(DateTime.Now)
                };
                context.CosuriDeCumparaturis.Add(cos);
                context.SaveChanges();

                context.CosIngredientes.Add(new CosIngrediente
                {
                    IdCos = cos.IdCos,
                    IdIngredient = 5,
                    Cantitate = 100,
                    Unitate = "g"
                });
                context.SaveChanges();

                var service = new CosService(context);

                var request = new AdaugaInCosRequest
                {
                    IdIngredient = 5,
                    Cantitate = 200,
                    Unitate = "g"
                };

                var result = await service.AdaugaIngredientInCosAsync(user.IdUtilizator, request);

                Assert.False(result.success);
                Assert.Equal("Ingredientul există deja în coș!", result.message);
            }
        }
    }
}
