using System;
using System.Collections.Generic;
using Aplicatie_Culinara_HealPlate.Models;
using Microsoft.EntityFrameworkCore;

namespace Aplicatie_Culinara_HealPlate.Data;

public partial class HealPlateDbContext : DbContext
{
    public HealPlateDbContext()
    {
    }

    public HealPlateDbContext(DbContextOptions<HealPlateDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alergeni> Alergenis { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<ColectiePersonala> ColectiePersonalas { get; set; }

    public virtual DbSet<ColectiePersonalaRetete> ColectiePersonalaRetetes { get; set; }

    public virtual DbSet<CosIngrediente> CosIngredientes { get; set; }

    public virtual DbSet<CosuriDeCumparaturi> CosuriDeCumparaturis { get; set; }

    public virtual DbSet<IngredientAlergeni> IngredientAlergenis { get; set; }

    public virtual DbSet<Ingrediente> Ingredientes { get; set; }

    public virtual DbSet<IntrebareAlergen> IntrebareAlergens { get; set; }

    public virtual DbSet<Notificari> Notificaris { get; set; }

    public virtual DbSet<PlanAlimentar> PlanAlimentars { get; set; }

    public virtual DbSet<Recenzii> Recenziis { get; set; }

    public virtual DbSet<RetetaIngrediente> RetetaIngredientes { get; set; }

    public virtual DbSet<Retete> Retetes { get; set; }

    public virtual DbSet<RezultatTestAlergeni> RezultatTestAlergenis { get; set; }

    public virtual DbSet<UtilizatorAlergeni> UtilizatorAlergenis { get; set; }

    public virtual DbSet<Utilizatori> Utilizatoris { get; set; }

    public virtual DbSet<VariantaIntrebareAlergen> VariantaIntrebareAlergens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=DefaultConnection");
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alergeni>(entity =>
        {
            entity.HasKey(e => e.IdAlergen).HasName("PK__Alergeni__874F405352E5B3A3");

            entity.ToTable("Alergeni");

            entity.HasIndex(e => e.NumeAlergen, "UQ__Alergeni__91654A1D6F4362C6").IsUnique();

            entity.Property(e => e.IdAlergen).HasColumnName("ID_Alergen");
            entity.Property(e => e.NumeAlergen)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Nume_Alergen");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ColectiePersonala>(entity =>
        {
            entity.HasKey(e => e.IdColectie).HasName("PK__Colectie__E9C279707414B7C4");

            entity.ToTable("ColectiePersonala");

            entity.HasIndex(e => e.IdUtilizator, "UQ_Colectie_Unica_Pentru_Utilizator").IsUnique();

            entity.Property(e => e.IdColectie).HasColumnName("ID_Colectie");
            entity.Property(e => e.DataAdaugare).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.IdUtilizator).HasColumnName("ID_Utilizator");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithOne(p => p.ColectiePersonala)
                .HasForeignKey<ColectiePersonala>(d => d.IdUtilizator)
                .HasConstraintName("FK__ColectieP__ID_Ut__31B762FC");
        });

        modelBuilder.Entity<ColectiePersonalaRetete>(entity =>
        {
            entity.HasKey(e => e.IdColectieReteta).HasName("PK__Colectie__DE9B35C2CBEE06E1");

            entity.ToTable("ColectiePersonala_Retete");

            entity.Property(e => e.IdColectieReteta).HasColumnName("ID_ColectieReteta");
            entity.Property(e => e.IdColectie).HasColumnName("ID_Colectie");
            entity.Property(e => e.IdReteta).HasColumnName("ID_Reteta");

            entity.HasOne(d => d.IdColectieNavigation).WithMany(p => p.ColectiePersonalaRetetes)
                .HasForeignKey(d => d.IdColectie)
                .HasConstraintName("FK__ColectieP__ID_Co__3493CFA7");

            entity.HasOne(d => d.IdRetetaNavigation).WithMany(p => p.ColectiePersonalaRetetes)
                .HasForeignKey(d => d.IdReteta)
                .HasConstraintName("FK__ColectieP__ID_Re__3587F3E0");
        });

        modelBuilder.Entity<CosIngrediente>(entity =>
        {
            entity.HasKey(e => e.IdCi).HasName("PK__Cos_Ingr__8B622F8261381749");

            entity.ToTable("Cos_Ingrediente");

            entity.Property(e => e.IdCi).HasColumnName("ID_CI");
            entity.Property(e => e.Cantitate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdCos).HasColumnName("ID_Cos");
            entity.Property(e => e.IdIngredient).HasColumnName("ID_Ingredient");
            entity.Property(e => e.Unitate)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCosNavigation).WithMany(p => p.CosIngredientes)
                .HasForeignKey(d => d.IdCos)
                .HasConstraintName("FK__Cos_Ingre__ID_Co__2BFE89A6");

            entity.HasOne(d => d.IdIngredientNavigation).WithMany(p => p.CosIngredientes)
                .HasForeignKey(d => d.IdIngredient)
                .HasConstraintName("FK__Cos_Ingre__ID_In__2CF2ADDF");
        });

        modelBuilder.Entity<CosuriDeCumparaturi>(entity =>
        {
            entity.HasKey(e => e.IdCos).HasName("PK__CosuriDe__2BFE64394EE8D6AF");

            entity.ToTable("CosuriDeCumparaturi");

            entity.HasIndex(e => e.IdUtilizator, "UQ_Utilizator_SingurCos").IsUnique();

            entity.Property(e => e.IdCos).HasColumnName("ID_Cos");
            entity.Property(e => e.DataCreare).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.IdUtilizator).HasColumnName("ID_Utilizator");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithOne(p => p.CosuriDeCumparaturi)
                .HasForeignKey<CosuriDeCumparaturi>(d => d.IdUtilizator)
                .HasConstraintName("FK__CosuriDeC__ID_Ut__29221CFB");
        });

        modelBuilder.Entity<IngredientAlergeni>(entity =>
        {
            entity.HasKey(e => e.IdIa).HasName("PK__Ingredie__8B62FCC21C80C76A");

            entity.ToTable("Ingredient_Alergeni");

            entity.Property(e => e.IdIa).HasColumnName("ID_IA");
            entity.Property(e => e.IdAlergen).HasColumnName("ID_Alergen");
            entity.Property(e => e.IdIngredient).HasColumnName("ID_Ingredient");

            entity.HasOne(d => d.IdAlergenNavigation).WithMany(p => p.IngredientAlergenis)
                .HasForeignKey(d => d.IdAlergen)
                .HasConstraintName("FK__Ingredien__ID_Al__1EA48E88");

            entity.HasOne(d => d.IdIngredientNavigation).WithMany(p => p.IngredientAlergenis)
                .HasForeignKey(d => d.IdIngredient)
                .HasConstraintName("FK__Ingredien__ID_In__1DB06A4F");
        });

        modelBuilder.Entity<Ingrediente>(entity =>
        {
            entity.HasKey(e => e.IdIngredient).HasName("PK__Ingredie__0F0820B4E607CC4B");

            entity.ToTable("Ingrediente");

            entity.HasIndex(e => e.Nume, "UQ__Ingredie__77B0141F54D708F4").IsUnique();

            entity.Property(e => e.IdIngredient).HasColumnName("ID_Ingredient");
            entity.Property(e => e.Nume)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<IntrebareAlergen>(entity =>
        {
            entity.HasKey(e => e.IdIntrebare).HasName("PK__Intrebar__F0B2F47648609E2F");

            entity.ToTable("IntrebareAlergen");

            entity.Property(e => e.Text).HasMaxLength(500);
        });

        modelBuilder.Entity<Notificari>(entity =>
        {
            entity.HasKey(e => e.IdNotificare).HasName("PK__Notifica__03EEEB10A3E3AD60");

            entity.ToTable("Notificari");

            entity.Property(e => e.DataCreare)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Vizualizat).HasDefaultValue(false);

            entity.HasOne(d => d.IdRetetaNavigation).WithMany(p => p.Notificaris)
                .HasForeignKey(d => d.IdReteta)
                .HasConstraintName("FK__Notificar__IdRet__11158940");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithMany(p => p.Notificaris)
                .HasForeignKey(d => d.IdUtilizator)
                .HasConstraintName("FK__Notificar__IdUti__10216507");
        });

        modelBuilder.Entity<PlanAlimentar>(entity =>
        {
            entity.HasKey(e => e.IdPlan).HasName("PK__PlanAlim__B4D79D370F5AB85A");

            entity.ToTable("PlanAlimentar");

            entity.Property(e => e.IdPlan).HasColumnName("ID_Plan");
            entity.Property(e => e.IdCina).HasColumnName("ID_Cina");
            entity.Property(e => e.IdDesert).HasColumnName("ID_Desert");
            entity.Property(e => e.IdGustare).HasColumnName("ID_Gustare");
            entity.Property(e => e.IdMicDeJun).HasColumnName("ID_MicDeJun");
            entity.Property(e => e.IdPranz).HasColumnName("ID_Pranz");
            entity.Property(e => e.IdUtilizator).HasColumnName("ID_Utilizator");

            entity.HasOne(d => d.IdCinaNavigation).WithMany(p => p.PlanAlimentarIdCinaNavigations)
                .HasForeignKey(d => d.IdCina)
                .HasConstraintName("FK__PlanAlime__ID_Ci__29E1370A");

            entity.HasOne(d => d.IdDesertNavigation).WithMany(p => p.PlanAlimentarIdDesertNavigations)
                .HasForeignKey(d => d.IdDesert)
                .HasConstraintName("FK__PlanAlime__ID_De__27F8EE98");

            entity.HasOne(d => d.IdGustareNavigation).WithMany(p => p.PlanAlimentarIdGustareNavigations)
                .HasForeignKey(d => d.IdGustare)
                .HasConstraintName("FK__PlanAlime__ID_Gu__28ED12D1");

            entity.HasOne(d => d.IdMicDeJunNavigation).WithMany(p => p.PlanAlimentarIdMicDeJunNavigations)
                .HasForeignKey(d => d.IdMicDeJun)
                .HasConstraintName("FK__PlanAlime__ID_Mi__2610A626");

            entity.HasOne(d => d.IdPranzNavigation).WithMany(p => p.PlanAlimentarIdPranzNavigations)
                .HasForeignKey(d => d.IdPranz)
                .HasConstraintName("FK__PlanAlime__ID_Pr__2704CA5F");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithMany(p => p.PlanAlimentars)
                .HasForeignKey(d => d.IdUtilizator)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__PlanAlime__ID_Ut__251C81ED");
        });

        modelBuilder.Entity<Recenzii>(entity =>
        {
            entity.HasKey(e => e.IdRecenzie).HasName("PK__Recenzii__15081335FE29DAAE");

            entity.ToTable("Recenzii");

            entity.HasIndex(e => new { e.IdUtilizator, e.IdReteta }, "UQ_Utilizator_Reteta").IsUnique();

            entity.Property(e => e.IdRecenzie).HasColumnName("ID_Recenzie");
            entity.Property(e => e.DataRecenzie).HasDefaultValueSql("(CONVERT([date],getdate()))");
            entity.Property(e => e.IdReteta).HasColumnName("ID_Reteta");
            entity.Property(e => e.IdUtilizator).HasColumnName("ID_Utilizator");
            entity.Property(e => e.TextRecenzie).HasColumnType("text");

            entity.HasOne(d => d.IdRetetaNavigation).WithMany(p => p.Recenziis)
                .HasForeignKey(d => d.IdReteta)
                .HasConstraintName("FK__Recenzii__ID_Ret__245D67DE");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithMany(p => p.Recenziis)
                .HasForeignKey(d => d.IdUtilizator)
                .HasConstraintName("FK__Recenzii__ID_Uti__236943A5");
        });

        modelBuilder.Entity<RetetaIngrediente>(entity =>
        {
            entity.HasKey(e => e.IdRi).HasName("PK__Reteta_I__8B6381F58C927816");

            entity.ToTable("Reteta_Ingrediente");

            entity.Property(e => e.IdRi).HasColumnName("ID_RI");
            entity.Property(e => e.Cantitate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdIngredient).HasColumnName("ID_Ingredient");
            entity.Property(e => e.IdReteta).HasColumnName("ID_Reteta");
            entity.Property(e => e.Unitate)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdIngredientNavigation).WithMany(p => p.RetetaIngredientes)
                .HasForeignKey(d => d.IdIngredient)
                .HasConstraintName("FK__Reteta_In__ID_In__1AD3FDA4");

            entity.HasOne(d => d.IdRetetaNavigation).WithMany(p => p.RetetaIngredientes)
                .HasForeignKey(d => d.IdReteta)
                .HasConstraintName("FK__Reteta_In__ID_Re__19DFD96B");
        });

        modelBuilder.Entity<Retete>(entity =>
        {
            entity.HasKey(e => e.IdReteta).HasName("PK__Retete__2F6450633F4D7924");

            entity.ToTable("Retete", tb => tb.HasTrigger("trg_SetNullOnDeleteReteta"));

            entity.Property(e => e.IdReteta).HasColumnName("ID_Reteta");
           // entity.Property(e => e.Aprobata).HasDefaultValue(false);
            entity.Property(e => e.Categorie)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Descriere).HasColumnType("text");
            entity.Property(e => e.Imagine)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ModDePreparare).HasColumnType("text");
            entity.Property(e => e.Titlu)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RezultatTestAlergeni>(entity =>
        {
            entity.HasKey(e => e.IdRezultat).HasName("PK__Rezultat__45050D58BFF461A6");

            entity.ToTable("RezultatTestAlergeni");

            entity.Property(e => e.DataTest)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Recomandare).HasMaxLength(500);

            entity.HasOne(d => d.IdAlergenNavigation).WithMany(p => p.RezultatTestAlergenis)
                .HasForeignKey(d => d.IdAlergen)
                .HasConstraintName("FK__RezultatT__IdAle__5D60DB10");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithMany(p => p.RezultatTestAlergenis)
                .HasForeignKey(d => d.IdUtilizator)
                .HasConstraintName("FK__RezultatT__IdUti__5C6CB6D7");
        });

        modelBuilder.Entity<UtilizatorAlergeni>(entity =>
        {
            entity.HasKey(e => e.IdUa).HasName("PK__Utilizat__8B625951E44A9A1C");

            entity.ToTable("Utilizator_Alergeni");

            entity.Property(e => e.IdUa).HasColumnName("ID_UA");
            entity.Property(e => e.IdAlergen).HasColumnName("ID_Alergen");
            entity.Property(e => e.IdUtilizator).HasColumnName("ID_Utilizator");

            entity.HasOne(d => d.IdAlergenNavigation).WithMany(p => p.UtilizatorAlergenis)
                .HasForeignKey(d => d.IdAlergen)
                .HasConstraintName("FK__Utilizato__ID_Al__123EB7A3");

            entity.HasOne(d => d.IdUtilizatorNavigation).WithMany(p => p.UtilizatorAlergenis)
                .HasForeignKey(d => d.IdUtilizator)
                .HasConstraintName("FK__Utilizato__ID_Ut__114A936A");
        });

        modelBuilder.Entity<Utilizatori>(entity =>
        {
            entity.HasKey(e => e.IdUtilizator).HasName("PK__Utilizat__3E67D806CEFE7F7A");

            entity.ToTable("Utilizatori");

            entity.HasIndex(e => e.Username, "UQ__Utilizat__536C85E4DB025757").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Utilizat__A9D105346351D6F1").IsUnique();

            entity.Property(e => e.IdUtilizator).HasColumnName("ID_Utilizator");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nume)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Parola)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Prenume)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Utilizator");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VariantaIntrebareAlergen>(entity =>
        {
            entity.HasKey(e => e.IdVarianta).HasName("PK__Varianta__4ACF8F0B8F7300BE");

            entity.ToTable("VariantaIntrebareAlergen");

            entity.Property(e => e.Text).HasMaxLength(200);

            entity.HasOne(d => d.IdAlergenVizatNavigation).WithMany(p => p.VariantaIntrebareAlergens)
                .HasForeignKey(d => d.IdAlergenVizat)
                .HasConstraintName("FK__VariantaI__IdAle__589C25F3");

            entity.HasOne(d => d.IdIntrebareNavigation).WithMany(p => p.VariantaIntrebareAlergens)
                .HasForeignKey(d => d.IdIntrebare)
                .HasConstraintName("FK__VariantaI__IdInt__57A801BA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
