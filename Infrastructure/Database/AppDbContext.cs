using Microsoft.EntityFrameworkCore;
using M_One_Layer3.Domain;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Person> Persons { get; set; }
    public DbSet<BiometricTemplate> BiometricTemplates { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasMany(p => p.Biometrics)
            .WithOne(b => b.Person)
            .HasForeignKey(b => b.PersonId);

        base.OnModelCreating(modelBuilder);
    }
}
