namespace Quickbird.Models
{
    using Qb.Poco.User;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Qb.Poco;
    using Qb.Poco.Global;

    public class QbDbContext : DbContext
    {
        public const string FileName = "maindb.db";

        public DbSet<CropCycle> CropCycles { get; set; }
        public DbSet<CropType> CropTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorHistory> SensorsHistory { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }

        /// <summary>Set filename for db.</summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={FileName}");
        }

        /// <summary>Configure the database model.</summary>
        /// <param name="builder">EF model builder.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Only entity with pk that is not ID.
            builder.Entity<CropType>().HasKey(ct => ct.Name);

            // A weak unenforced foreign key to Person.
            builder.Entity<CropType>().Property(ct => ct.CreatedBy).IsRequired(false);

            // Composite key.
            builder.Entity<SensorHistory>().HasKey(sd => new { sd.SensorId, sd.UtcDate});

            // Set optional foreign key, defaults delete to restrict.
            builder.Entity<Location>().Property(loc => loc.PersonId).IsRequired(false);
            // Changed behaviour to setnull, allows remaking people without hurtning data.
            builder.Entity<Location>()
                .HasOne(loc => loc.Person)
                .WithMany(p => p.Locations)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(builder);
        }
    }
}
