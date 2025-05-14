using Aplicatie_Culinara_HealPlate.Models;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public interface IRetetaService//interfata pentru serviciu
    {
        Retete GetRetetaById(int id);
        Task<bool> ApprovePost1Async(int idReteta);
        Task<bool> RejectPostAsync(int idReteta);
        Task<(bool success, string message)> AddToCollectionAsync(int? userId, int idReteta);
        Task<(bool success, string message)> RemoveFromCollectionAsync(int? userId, int idReteta);
        Task<(bool success, string message)> DeleteRecipeAsync(int idReteta);
        Task<List<Retete>> GetFilteredReteteAsync(int? userId, string? categorie, string? searchQuery);
        Task<Dictionary<int, bool>> GetEsteInColectieAsync(int userId, List<Retete> retete);
    }
}
