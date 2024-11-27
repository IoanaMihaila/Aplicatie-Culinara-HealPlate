using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class RetetaIngrediente
{
    public int IdRi { get; set; }

    public int IdReteta { get; set; }

    public int IdIngredient { get; set; }

    public decimal? Cantitate { get; set; }

    public string? Unitate { get; set; }

    public virtual Ingrediente IdIngredientNavigation { get; set; } = null!;

    public virtual Retete IdRetetaNavigation { get; set; } = null!;
}
