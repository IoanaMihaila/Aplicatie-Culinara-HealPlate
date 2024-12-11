using Microsoft.AspNetCore.Mvc;

namespace Aplicatie_Culinara_HealPlate.Models
{
    public class AddToCollectionRequest
    {
        [BindProperty]
        public int IdReteta { get; set; }
    }
}
