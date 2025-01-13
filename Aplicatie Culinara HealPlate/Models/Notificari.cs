using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Notificari
{
    public int IdNotificare { get; set; }

    public string? Mesaj { get; set; }

    public DateTime? DataCreare { get; set; }

    public int IdUtilizator { get; set; }

    public int IdReteta { get; set; }

    public bool? Vizualizat { get; set; }

    public virtual Retete IdRetetaNavigation { get; set; } = null!;

    public virtual Utilizatori IdUtilizatorNavigation { get; set; } = null!;
}
