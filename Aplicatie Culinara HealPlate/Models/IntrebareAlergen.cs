using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class IntrebareAlergen
{
    public int IdIntrebare { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<VariantaIntrebareAlergen> VariantaIntrebareAlergens { get; set; } = new List<VariantaIntrebareAlergen>();
}
