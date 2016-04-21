namespace Agronomist.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.Data.Entity;

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
        public DbSet<BaseEntity> BaseEntities { get; set; }
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
            modelBuilder.Entity<ControlHistory>().HasKey(ct => new {ct.ControllableID, ct.DateTime});

            modelBuilder.Entity<Greenhouse>().HasRequired(gh => gh.Person)
                .WithMany(p => p.Greenhouses).HasForeignKey(gh => gh.PersonId).WillCascadeOnDelete(false);


            modelBuilder.Entity<Controllable>().HasOptional(ct => ct.Relay)
                .WithOptionalDependent(r => r.Controlable)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasKey(p => p.ID)
                .Property(p => p.ID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<SensorData>().HasKey(sd => new {sd.SensorID, sd.DateTime})
                .Property(sd => sd.GreenhouseID).IsOptional();
            modelBuilder.Entity<Greenhouse>().HasMany(gh => gh.SensorData)
                .WithOptional(sd => sd.Greenhouse)
                .WillCascadeOnDelete(false);
        }
    }
}