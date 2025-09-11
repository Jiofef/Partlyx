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
            .ValueGeneratedOnAdd();
            rb.Property<Guid?>("ResourceUid").HasColumnType("BLOB");
            rb.HasMany(x => x.Components)
                .WithOne(x => x.ParentRecipe)
                .HasForeignKey("RecipeUid")
                .OnDelete(DeleteBehavior.Cascade);

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
            .ValueGeneratedOnAdd();
            cb.Property<Guid?>("RecipeUid").HasColumnType("BLOB");
            cb.Property<Guid?>("ComponentResourceUid").HasColumnType("BLOB");

            cb.HasOne(c => c.ComponentResource)
                .WithMany()
                .HasForeignKey("ComponentResourceUid")
                .OnDelete(DeleteBehavior.Restrict);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Since Parts create UIDs in themselves, the BD thinks that the object has already been created. Here we tell the DB that having a set Uid does not mean that the object has already been created.
        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is IPart))
        {
            if (entry.State == EntityState.Modified)
            {
                var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue;
                if (key != null)
                {
                    var dbValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                    if (dbValues == null)
                        entry.State = EntityState.Added;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}