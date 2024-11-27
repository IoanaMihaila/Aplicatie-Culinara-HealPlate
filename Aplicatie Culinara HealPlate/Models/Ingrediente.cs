using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Ingrediente
{
    public int IdIngredient { get; set; }

    public string Nume { get; set; } = null!;

    public virtual ICollection<CosIngrediente> CosIngredientes { get; set; } = new List<CosIngrediente>();

    public virtual ICollection<IngredientAlergeni> IngredientAlergenis { get; set; } = new List<IngredientAlergeni>();

    public virtual ICollection<RetetaIngrediente> RetetaIngredientes { get; set; } = new List<RetetaIngrediente>();
}
