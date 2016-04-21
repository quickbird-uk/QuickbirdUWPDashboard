namespace Agronomist.Models
{
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Metadata;

    public class MainDbContext : DbContext
    {
        public DbSet<ControlType> ControlTypes { get; set; }
        public DbSet<ParamAtPlace> ParamAtPlaces { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<PlacementType> PlacementTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Relay> Relays { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<ControlHistory> ControlHistories { get; set; }
        public DbSet<Controllable> Controllables { get; set; }
        public DbSet<CropCycle> CropCycles { get; set; }
        public DbSet<CropType> CropTypes { get; set; }
        public DbSet<Greenhouse> Greenhouses { get; set; }
        public DbSet<SensorData> SensorDatas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=maindb.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ControlHistory>()
                .HasKey(ct => new {ct.ControllableID, ct.DateTime});

            modelBuilder.Entity<Greenhouse>()
                .HasOne(gh => gh.Person)
                .WithMany(p => p.Greenhouses)
                .HasForeignKey(gh => gh.PersonId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Controllable>()
                .HasOne(ct => ct.Relay)
                .WithOne(r => r.Controlable)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SensorData>()
                .HasKey(sd => new {sd.SensorID, sd.DateTime});

            modelBuilder.Entity<Greenhouse>()
                .HasMany(gh => gh.SensorData)
                .WithOne(sd => sd.Greenhouse)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}