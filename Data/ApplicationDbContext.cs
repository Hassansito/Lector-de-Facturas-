using Microsoft.EntityFrameworkCore;
using Models.Entities;
namespace Data
{
    public interface IAuditableEntity
    {
        DateTime CreatedDate { get; set; }
    }
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<ClienteEntity> Cliente { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(u => u.NombreUsuario).IsUnique();

                // Opción 1: Generar Id desde código (recomendado para empezar)
                // No agregues ninguna configuración para Id.

                // Opción 2: Si quieres que la BD genere el UUID automáticamente:
                // entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });

            modelBuilder.Entity<Usuario>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<ClienteEntity>(entity =>
            {
                entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            });
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                }               
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
