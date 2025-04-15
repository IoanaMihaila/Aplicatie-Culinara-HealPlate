using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class VariantaIntrebareAlergen
{
    public int IdVarianta { get; set; }

    public int IdIntrebare { get; set; }

    public string Text { get; set; } = null!;

    public int Punctaj { get; set; }

    public int IdAlergenVizat { get; set; }

    public virtual Alergeni IdAlergenVizatNavigation { get; set; } = null!;

    public virtual IntrebareAlergen IdIntrebareNavigation { get; set; } = null!;
}
