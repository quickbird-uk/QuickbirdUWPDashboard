namespace Quickbird.Models
{
    using DbStructure;
    using DbStructure.Global;
    using DbStructure.User;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;

    public class MainDbContext : DbContext
    {
        public const string FileName = "maindb.db";

        public DbSet<CropCycle> CropCycles { get; set; }
        public DbSet<CropType> CropTypes { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<RelayHistory> RelayHistory { get; set; }
        public DbSet<Relay> Relays { get; set; }
        public DbSet<RelayType> RelayTypes { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorHistory> SensorsHistory { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }

        /// <summary>
        ///     Set filename for db.
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={FileName}");
        }

        /// <summary>
        ///     Configure the database model.
        /// </summary>
        /// <param name="mb"></param>
        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<RelayHistory>().HasKey(rh => new {rh.RelayID, rh.TimeStamp});
            mb.Entity<SensorHistory>().HasKey(sh => new {sh.SensorID, sh.TimeStamp});

            mb.Entity<Location>()
                .HasOne(l => l.Person)
                .WithMany(p => p.Locations)
                .HasForeignKey(l => l.PersonId).IsRequired()
                .OnDelete(DeleteBehavior.SetNull);

            // Skip the person init, it can't be editied on this side anyway.
            mb.Entity<Sensor>().Property(s => s.AlertHigh).IsRequired(false);
            mb.Entity<Sensor>().Property(s => s.AlertLow).IsRequired(false);

            mb.Entity<Location>()
                .HasMany(loc => loc.SensorHistory)
                .WithOne(sh => sh.Location)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            mb.Entity<CropType>()
                .HasKey(ct => ct.Name);

            mb.Entity<CropType>()
                .Property(ct => ct.Name)
                .ValueGeneratedNever();

            mb.Entity<CropType>()
                .HasMany(ct => ct.CropCycles)
                .WithOne(cc => cc.CropType).IsRequired()
                .HasForeignKey(cc => cc.CropTypeName);

            mb.Entity<RelayHistory>().Ignore(rh => rh.Data);
            mb.Entity<SensorHistory>().Ignore(sh => sh.Data);
        }
    }
}