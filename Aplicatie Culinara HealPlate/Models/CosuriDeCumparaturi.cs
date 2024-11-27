using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class CosuriDeCumparaturi
{
    public int IdCos { get; set; }

    public int IdUtilizator { get; set; }

    public DateOnly? DataCreare { get; set; }

    public virtual ICollection<CosIngrediente> CosIngredientes { get; set; } = new List<CosIngrediente>();

    public virtual Utilizatori IdUtilizatorNavigation { get; set; } = null!;
}
