using System;
using System.Collections.Generic;

namespace Aplicatie_Culinara_HealPlate.Models;

public partial class Recenzii
{
    public int IdRecenzie { get; set; }

    public int IdUtilizator { get; set; }

    public int IdReteta { get; set; }

    public string? TextRecenzie { get; set; }

    public DateOnly? DataRecenzie { get; set; }
    public int Scor { get; set; }

    public virtual Retete IdRetetaNavigation { get; set; } = null!;

    public virtual Utilizatori IdUtilizatorNavigation { get; set; } = null!;
    public string ScorSelectOptions()
    {
        var options = "";
        for (int i = 1; i <= 5; i++)
        {
            var selected = (i == Scor) ? "selected" : "";
            options += $"<option value='{i}' {selected}>{i}</option>";
        }
        return options;
    }
}
