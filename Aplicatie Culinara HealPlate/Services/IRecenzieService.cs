using Aplicatie_Culinara_HealPlate.Models;

namespace Aplicatie_Culinara_HealPlate.Services
{
    public interface IRecenzieService
    {
        Task AddRecenzieAsync(Recenzii recenzie);
        public Recenzii GetRecenzieByUtilizatorSiReteta(int idUtilizator, int idReteta);
    }
}
