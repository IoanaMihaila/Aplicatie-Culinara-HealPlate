using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Alergeni
{
    public int IdAlergen { get; set; }

    public string NumeAlergen { get; set; } = null!;

    public virtual ICollection<IngredientAlergeni> IngredientAlergenis { get; set; } = new List<IngredientAlergeni>();

    public virtual ICollection<UtilizatorAlergeni> UtilizatorAlergenis { get; set; } = new List<UtilizatorAlergeni>();
}
