using Microsoft.EntityFrameworkCore;
using Partlyx.Core;
using System.Reflection.Metadata;

namespace Partlyx.Data
{
    public class PartlyxDBContext : DbContext, IDisposable
    {
        public DbSet<Resource> Resources { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=partlyxmain.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>(r =>
            {
                r.HasKey(x => x.Uid);
                r.Property(x => x.Uid).HasColumnName("Uid").HasColumnType("BLOB");

                r.Property(x => x.Name).IsRequired();

                r.Property(x => x.DefaultRecipeUid).
                HasColumnName("DefaultRecipeUid").
                HasColumnType("BLOB");

                r.OwnsMany(x => x.Recipes, rb =>
                {
                    rb.WithOwner(x => x.ParentResource).HasForeignKey("ResourceUid");
                    rb.Property<Guid?>("ResourceUid").HasColumnType("BLOB");

                    rb.HasKey(x => x.Uid);
                    rb.Property(x => x.Uid).HasColumnName("Uid").HasColumnType("BLOB");
                    
                    rb.OwnsMany(x => x.Components, cb =>
                    {
                        cb.WithOwner(x => x.ParentRecipe).HasForeignKey("RecipeUid");
                        cb.Property<Guid?>("RecipeUid").HasColumnType("BLOB");

                        cb.HasKey(x => x.Uid);
                        cb.Property(x => x.Uid).HasColumnName("Uid").HasColumnType("BLOB");

                        cb.Property<Guid?>("ComponentResourceUid").HasColumnType("BLOB");

                        cb.HasOne(c=> c.ComponentResource)
                        .WithMany()
                        .HasForeignKey("ComponentResourceId")
                        .OnDelete(DeleteBehavior.Restrict);
                    }
                    );
                }
                );
            }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
