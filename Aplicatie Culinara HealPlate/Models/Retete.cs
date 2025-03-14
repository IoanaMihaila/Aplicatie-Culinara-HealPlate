using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Retete
{
    public int IdReteta { get; set; }

    public string Titlu { get; set; } = null!;

    public string Imagine { get; set; } = null!;

    public string Categorie { get; set; } = null!;

    public string Descriere { get; set; } = null!;

    public string ModDePreparare { get; set; } = null!;

    public int TimpPreparare { get; set; }

    public bool? Aprobata { get; set; }

    public virtual ICollection<ColectiePersonalaRetete> ColectiePersonalaRetetes { get; set; } = new List<ColectiePersonalaRetete>();

    public virtual ICollection<Notificari> Notificaris { get; set; } = new List<Notificari>();

    [JsonIgnore]
    public virtual ICollection<PlanAlimentar> PlanAlimentarIdCinaNavigations { get; set; } = new List<PlanAlimentar>();
    [JsonIgnore]
    public virtual ICollection<PlanAlimentar> PlanAlimentarIdDesertNavigations { get; set; } = new List<PlanAlimentar>();
    [JsonIgnore]
    public virtual ICollection<PlanAlimentar> PlanAlimentarIdGustareNavigations { get; set; } = new List<PlanAlimentar>();
    [JsonIgnore]
    public virtual ICollection<PlanAlimentar> PlanAlimentarIdMicDeJunNavigations { get; set; } = new List<PlanAlimentar>();
    [JsonIgnore]
    public virtual ICollection<PlanAlimentar> PlanAlimentarIdPranzNavigations { get; set; } = new List<PlanAlimentar>();

    public virtual ICollection<Recenzii> Recenziis { get; set; } = new List<Recenzii>();

    public virtual ICollection<RetetaIngrediente> RetetaIngredientes { get; set; } = new List<RetetaIngrediente>();
}
