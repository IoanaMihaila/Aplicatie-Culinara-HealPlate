using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class PlanAlimentar
{
    public int IdPlan { get; set; }

    public int? IdUtilizator { get; set; }

    public int? IdMicDeJun { get; set; }

    public int? IdPranz { get; set; }

    public int? IdDesert { get; set; }

    public int? IdGustare { get; set; }

    public int? IdCina { get; set; }

    public DateOnly Ziua { get; set; }

    public virtual Retete? IdCinaNavigation { get; set; }

    public virtual Retete? IdDesertNavigation { get; set; }

    public virtual Retete? IdGustareNavigation { get; set; }

    public virtual Retete? IdMicDeJunNavigation { get; set; }

    public virtual Retete? IdPranzNavigation { get; set; }

    public virtual Utilizatori? IdUtilizatorNavigation { get; set; }
}
