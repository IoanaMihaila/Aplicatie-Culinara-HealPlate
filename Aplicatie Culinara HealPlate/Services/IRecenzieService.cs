using Aplicatie_Culinara_HealPlate.Models;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public interface IRecenzieService
    {
        Task AddRecenzieAsync(Recenzii recenzie);
        public Recenzii GetRecenzieByUtilizatorSiReteta(int idUtilizator, int idReteta);
        Task DeleteRecenzieAsync(int idRecenzie);
        Task UpdateRecenzieAsync(int idRecenzie, string textNou, int scorNou);

    }
}
