using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Utilizatori
{
    [BindProperty]
    public int IdUtilizator { get; set; }
    [BindProperty]
    public string Nume { get; set; } = null!;
    [BindProperty]
    public string Prenume { get; set; } = null!;
    [BindProperty]
    public string Email { get; set; } = null!;
    [BindProperty]
    public string Username { get; set; } = null!;
    [BindProperty]
    public string Parola { get; set; } = null!;

    public virtual ColectiePersonala? ColectiePersonala { get; set; }

    public virtual CosuriDeCumparaturi? CosuriDeCumparaturi { get; set; }
    [BindProperty]
    public virtual ICollection<Recenzii> Recenziis { get; set; } = new List<Recenzii>();

    public virtual ICollection<UtilizatorAlergeni> UtilizatorAlergenis { get; set; } = new List<UtilizatorAlergeni>();
}
