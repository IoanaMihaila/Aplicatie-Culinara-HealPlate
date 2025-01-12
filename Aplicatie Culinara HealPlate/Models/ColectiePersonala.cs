using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class ColectiePersonala
{
    public int IdColectie { get; set; }

    public int IdUtilizator { get; set; }

    public DateOnly? DataAdaugare { get; set; }

    public virtual ICollection<ColectiePersonalaRetete> ColectiePersonalaRetetes { get; set; } = new List<ColectiePersonalaRetete>();

    public virtual Utilizatori IdUtilizatorNavigation { get; set; } = null!;
}
