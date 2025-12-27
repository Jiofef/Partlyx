using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using Partlyx.Core.Partlyx;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Partlyx.Infrastructure.Data;

public class PartlyxDBContext : DbContext, IDisposable
{
    public PartlyxDBContext(DbContextOptions<PartlyxDBContext> options)
        : base(options) { }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeComponent> RecipeComponents { get; set; }

    // Images
    public DbSet<PartlyxImage> Images { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(rb =>
        {
            rb.HasKey(r => r.Uid);
            rb.Property(r => r.Uid)
            .HasColumnName("Uid")
            .HasColumnType("BLOB")
            .ValueGeneratedOnAdd();
            rb.Property(r => r.Name).IsRequired();
            rb.Property(r => r.DefaultRecipeUid).HasColumnType("BLOB");
        });

        modelBuilder.Entity<Recipe>(rcb =>
        {
            rcb.ToTable("Recipes");
            rcb.HasKey(rc => rc.Uid);
            rcb.Property(rc => rc.Uid)
            .HasColumnName("Uid")
            .HasColumnType("BLOB")
            .ValueGeneratedNever();
            rcb.Property(rc => rc.Name).IsRequired();
            rcb.Property(rc => rc.IsReversible).HasDefaultValue(false);
            rcb.HasMany(rc => rc.Components)
                .WithOne(rc => rc.ParentRecipe)
                .HasForeignKey("RecipeUid")
                .OnDelete(DeleteBehavior.Cascade);

            rcb.Metadata.FindNavigation(nameof(Recipe.Components))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<RecipeComponent>(cb =>
        {
            cb.ToTable("RecipeComponents");
            cb.HasKey(c => c.Uid);
            cb.Property(c => c.Uid)
            .HasColumnName("Uid")
            .HasColumnType("BLOB")
            .ValueGeneratedNever();
            cb.Property(c => c.Quantity);
            cb.Property(c => c.IsOutput).HasDefaultValue(false);
            cb.Property<Guid?>("RecipeUid").HasColumnType("BLOB");
            cb.Property<Guid?>("ComponentResourceUid").HasColumnType("BLOB");

            cb.HasOne(c => c.ComponentResource)
                .WithMany()
                .HasForeignKey("ComponentResourceUid")
                .OnDelete(DeleteBehavior.Restrict);

            cb.Metadata.FindNavigation(nameof(RecipeComponent.ComponentResource))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            cb.Metadata.FindNavigation(nameof(RecipeComponent.ComponentResource))!
            .SetField("_componentResource");

            cb.Metadata.FindNavigation(nameof(RecipeComponent.ParentRecipe))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            cb.Metadata.FindNavigation(nameof(RecipeComponent.ParentRecipe))!
            .SetField("_parentRecipe");
        });

        modelBuilder.Entity<PartlyxImage>(ib =>
        {
            ib.ToTable("Images");
            ib.HasKey(i => i.Uid);

            ib.Property(i => i.Uid)
                .HasColumnType("BLOB")
                .ValueGeneratedOnAdd();

            ib.Property(i => i.Name)
                .HasColumnType("TEXT")
                .IsRequired();

            ib.Property(i => i.Content)
                .HasColumnType("BLOB")
                .IsRequired();

            ib.Property(i => i.CompressedContent)
                .HasColumnType("BLOB")
                .IsRequired();

            ib.Property(i => i.Hash)
                .HasColumnType("BLOB")
                .IsRequired();
            ib.HasIndex(i => i.Hash);

            ib.Property(i => i.Mime)
                .HasColumnType("TEXT")
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
