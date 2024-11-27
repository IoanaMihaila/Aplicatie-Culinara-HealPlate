using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class ColectiePersonalaRetete
{
    public int IdColectieReteta { get; set; }

    public int IdColectie { get; set; }

    public int IdReteta { get; set; }

    public virtual ColectiePersonala IdColectieNavigation { get; set; } = null!;

    public virtual Retete IdRetetaNavigation { get; set; } = null!;
}
