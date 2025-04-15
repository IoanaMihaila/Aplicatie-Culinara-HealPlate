using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class RezultatTestAlergeni
{
    public int IdRezultat { get; set; }

    public int IdUtilizator { get; set; }

    public int IdAlergen { get; set; }

    public DateTime? DataTest { get; set; }

    public int Scor { get; set; }

    public string? Recomandare { get; set; }

    public virtual Alergeni IdAlergenNavigation { get; set; } = null!;

    public virtual Utilizatori IdUtilizatorNavigation { get; set; } = null!;
}
