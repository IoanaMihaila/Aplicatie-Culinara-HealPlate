using Aplicatie_Culinara_HealPlate.Models;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public interface IRetetaService//interfata pentru serviciu
    {
        Retete GetRetetaById(int id);
        Task<bool> ApprovePost1Async(int idReteta);
        Task<bool> RejectPostAsync(int idReteta);
    }
}
