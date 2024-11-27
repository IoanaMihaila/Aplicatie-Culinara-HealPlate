using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class CosIngrediente
{
    public int IdCi { get; set; }

    public int IdCos { get; set; }

    public int IdIngredient { get; set; }

    public decimal? Cantitate { get; set; }

    public string? Unitate { get; set; }

    public virtual CosuriDeCumparaturi IdCosNavigation { get; set; } = null!;

    public virtual Ingrediente IdIngredientNavigation { get; set; } = null!;
}
