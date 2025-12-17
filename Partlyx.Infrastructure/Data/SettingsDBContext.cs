
using Microsoft.EntityFrameworkCore;
using Partlyx.Core.Settings;

namespace Partlyx.Infrastructure.Data;
public class SettingsDBContext : DbContext
{
    public SettingsDBContext(DbContextOptions<SettingsDBContext> options)
        : base(options) { }
    public DbSet<OptionEntity> Options { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var opt = modelBuilder.Entity<OptionEntity>();

        opt.HasKey(o => o.Id);

        opt.Property(o => o.Key)
            .IsRequired()
            .HasMaxLength(200);

        opt.HasIndex(o => o.Key)
           .IsUnique()
           ;

        opt.Property(o => o.ValueJson)
           .IsRequired()
           .HasColumnType("TEXT")
           .HasDefaultValue("{}");

        opt.ToTable(t => t.HasCheckConstraint("CK_OptionEntity_ValueJson_IsJson", "json_valid(ValueJson)"));

        opt.Property(o => o.TypeName)
           .IsRequired()
           .HasMaxLength(512);

        base.OnModelCreating(modelBuilder);
    }
}