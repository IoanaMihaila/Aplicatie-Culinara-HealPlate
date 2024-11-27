using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class IngredientAlergeni
{
    public int IdIa { get; set; }

    public int IdIngredient { get; set; }

    public int IdAlergen { get; set; }

    public virtual Alergeni IdAlergenNavigation { get; set; } = null!;

    public virtual Ingrediente IdIngredientNavigation { get; set; } = null!;
}
