namespace Agronomist.Models
{
    using System.Threading.Tasks;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.Data.Entity;
    using Microsoft.Data.Entity.Metadata;
    using NetLib;

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
            modelBuilder.Entity<CropType>()
                .HasKey(c => c.Name);

            modelBuilder.Entity<ControlHistory>()
                .HasKey(ct => new {ct.ControllableID, ct.DateTime});

            modelBuilder.Entity<Greenhouse>()
                .HasOne(gh => gh.Person)
                .WithMany(p => p.Greenhouses)
                .HasForeignKey(gh => gh.PersonId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.SetNull);

            //modelBuilder.Entity<Controllable>()
            //    .HasOne(ct => ct.Relay)
            //    .WithOne(r => r.)
            //    .IsRequired(false)
            //    .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<SensorData>()
                .HasKey(sd => new {sd.SensorID, sd.DateTime});

            modelBuilder.Entity<Greenhouse>()
                .HasMany(gh => gh.SensorData)
                .WithOne(sd => sd.Greenhouse)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }

        public async Task<string> FetchTable(string tableName, Creds cred = null)
        {
            const string baseUrl = "https://ghapi46azure.azurewebsites.net/api";
            string response;
            if(cred == null)
                response = await NetLib.Request.RequestTable(baseUrl, tableName);
            else
                response = await NetLib.Request.RequestTable(baseUrl, tableName, cred);
            return response;
        }

        public void PulAndPopulate()
        {
            //Reds first since they are an independent set, and they do not require authentication.
            // 1.PlacementType
            // 2.Parameter
            // 3.Subsystem
            // 4.Placement_has_Parameter
            // 5.ControlTypes
            //Now for the big part
            // 1.Greenhouse
            // 2.CropType
            // 3.Devices
            // 4.Cycle
            // 5.Sensors
            // 6.Relay
            // 7.Controllable
            // 8.ControlHistory
            // 9.SensorData

        }
    }
}