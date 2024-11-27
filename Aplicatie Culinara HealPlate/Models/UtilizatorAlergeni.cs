using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class UtilizatorAlergeni
{
    public int IdUa { get; set; }

    public int IdUtilizator { get; set; }

    public int IdAlergen { get; set; }

    public virtual Alergeni IdAlergenNavigation { get; set; } = null!;

    public virtual Utilizatori IdUtilizatorNavigation { get; set; } = null!;
}
