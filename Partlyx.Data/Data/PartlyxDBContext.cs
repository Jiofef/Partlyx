using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Partlyx.Infrastructure.Data;

public class PartlyxDBContext : DbContext, IDisposable
{
    public DbSet<Resource> Resources { get; set; }

    public PartlyxDBContext(DbContextOptions<PartlyxDBContext> options)
    : base(options)
    { }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeComponent> RecipeComponents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(r =>
        {
            r.HasKey(x => x.Uid);
            r.Property(x => x.Uid)
            .HasColumnName("Uid")
            .HasColumnType("BLOB")
            .ValueGeneratedOnAdd();
            r.Property(x => x.Name).IsRequired();
            r.Property(x => x.DefaultRecipeUid).HasColumnType("BLOB");
            r.HasMany(x => x.Recipes)
                .WithOne(x => x.ParentResource)
                .HasForeignKey("ResourceUid")
                .OnDelete(DeleteBehavior.Cascade);

            r.Metadata.FindNavigation(nameof(Resource.Recipes))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            r.Metadata.FindNavigation(nameof(Resource.Recipes))!
                .SetField("_recipes");
        });

        modelBuilder.Entity<Recipe>(rb =>
        {
            rb.ToTable("Recipes");
            rb.HasKey(x => x.Uid);
            rb.Property(x => x.Uid)
            .HasColumnName("Uid")
            .HasColumnType("BLOB")
            .ValueGeneratedNever();
            rb.Property<Guid?>("ResourceUid").HasColumnType("BLOB");
            rb.HasMany(x => x.Components)
                .WithOne(x => x.ParentRecipe)
                .HasForeignKey("RecipeUid")
                .OnDelete(DeleteBehavior.Cascade);

            rb.Metadata.FindNavigation(nameof(Recipe.ParentResource))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            rb.Metadata.FindNavigation(nameof(Recipe.ParentResource))!
            .SetField("_parentResource");

            rb.Metadata.FindNavigation(nameof(Recipe.Components))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            rb.Metadata.FindNavigation(nameof(Recipe.Components))!
                .SetField("_components");

            
        });

        modelBuilder.Entity<RecipeComponent>(cb =>
        {
            cb.ToTable("RecipeComponents");
            cb.HasKey(x => x.Uid);
            cb.Property(x => x.Uid)
            .HasColumnName("Uid")
            .HasColumnType("BLOB")
            .ValueGeneratedNever();
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

        base.OnModelCreating(modelBuilder);
    }
}