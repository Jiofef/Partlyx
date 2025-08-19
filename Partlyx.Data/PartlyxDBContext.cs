using Microsoft.EntityFrameworkCore;
using Partlyx.Core;

namespace Partlyx.Data
{
    public class PartlyxDBContext : DbContext
    {
        public DbSet<Resource> Resources { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=sqltest.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Resource>(r =>
            {
                r.HasKey(x => x.Id);
                r.Property(x => x.Name).IsRequired();

                r.OwnsMany(x => x.Recipes, rb =>
                {
                    rb.WithOwner().HasForeignKey("ResourceId");
                    rb.Property<int>("Id");
                    rb.HasKey("Id");
                    rb.OwnsMany(x => x.Components, cb =>
                    {
                        cb.WithOwner().HasForeignKey("RecipeId");
                        cb.Property<int>("Id");
                        cb.HasKey("Id");

                        cb.HasOne(c=> c.ComponentResource).WithMany()
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
