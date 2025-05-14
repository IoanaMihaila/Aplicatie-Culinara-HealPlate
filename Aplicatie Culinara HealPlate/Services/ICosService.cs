using Aplicatie_Culinara_HealPlate.Pages;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public interface ICosService
    {
        Task<(bool success, string message)> AdaugaIngredientInCosAsync(int userId, AdaugaInCosRequest request);
    }
}
