using System;
using System.Collections.Generic;

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

    public virtual ICollection<ColectiePersonalaRetete> ColectiePersonalaRetetes { get; set; } = new List<ColectiePersonalaRetete>();

    public virtual ICollection<Recenzii> Recenziis { get; set; } = new List<Recenzii>();

    public virtual ICollection<RetetaIngrediente> RetetaIngredientes { get; set; } = new List<RetetaIngrediente>();
}
