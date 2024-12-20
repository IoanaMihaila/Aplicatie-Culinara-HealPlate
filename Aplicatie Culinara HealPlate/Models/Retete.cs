using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Retete
{
    [BindProperty]
    public int IdReteta { get; set; }
    [BindProperty]
    public string Titlu { get; set; } = null!;
    [BindProperty]
    public string Imagine { get; set; } = null!;
    [BindProperty]
    public string Categorie { get; set; } = null!;
    [BindProperty]
    public string Descriere { get; set; } = null!;
    [BindProperty]
    public string ModDePreparare { get; set; } = null!;
    [BindProperty]
    public int TimpPreparare { get; set; }
    [BindProperty]
    public bool Aprobata { get; set; } = false;

    public virtual ICollection<ColectiePersonalaRetete> ColectiePersonalaRetetes { get; set; } = new List<ColectiePersonalaRetete>();
    [BindProperty]
    public virtual ICollection<Recenzii> Recenziis { get; set; } = new List<Recenzii>();

    public virtual ICollection<RetetaIngrediente> RetetaIngredientes { get; set; } = new List<RetetaIngrediente>();

}
