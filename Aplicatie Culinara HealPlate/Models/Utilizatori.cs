using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Utilizatori
{
    public int IdUtilizator { get; set; }

    public string Nume { get; set; } = null!;

    public string Prenume { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Parola { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public virtual ColectiePersonala? ColectiePersonala { get; set; }

    public virtual CosuriDeCumparaturi? CosuriDeCumparaturi { get; set; }

    public virtual ICollection<Notificari> Notificaris { get; set; } = new List<Notificari>();

    public virtual ICollection<PlanAlimentar> PlanAlimentars { get; set; } = new List<PlanAlimentar>();

    public virtual ICollection<Recenzii> Recenziis { get; set; } = new List<Recenzii>();

    public virtual ICollection<UtilizatorAlergeni> UtilizatorAlergenis { get; set; } = new List<UtilizatorAlergeni>();
}
