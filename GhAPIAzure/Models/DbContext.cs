using DatabasePOCOs;
using DatabasePOCOs.Global;
using DatabasePOCOs.User;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace GhAPIAzure.Models
{
    class DbContext : System.Data.Entity.DbContext
    {
        public System.Data.Entity.DbSet<ControlType> ControlTypes { get; set; }
        public System.Data.Entity.DbSet<Parameter> Parameters { get; set; }
        public System.Data.Entity.DbSet<PlacementType> PlacementTypes { get; set; }
        public System.Data.Entity.DbSet<ParamAtPlace> ParamsAtPlaces { get; set; }
        public System.Data.Entity.DbSet<Subsystem> Subsystems { get; set; }

        //User Data
        public System.Data.Entity.DbSet<Person> People { get; set; }
        public System.Data.Entity.DbSet<Greenhouse> Greenhouses { get; set; }
        public System.Data.Entity.DbSet<CropCycle> CropCycles { get; set; }
        public System.Data.Entity.DbSet<CropType> CropTypes { get; set; }
        public System.Data.Entity.DbSet<Controllable> Controllables { get; set; }
        public System.Data.Entity.DbSet<ControlHistory> ControlsHistory { get; set; }
        public System.Data.Entity.DbSet<SensorData> SensorsData { get; set; }

        //Device Data
        public System.Data.Entity.DbSet<Device> Devices{ get; set; }
        public System.Data.Entity.DbSet<Relay> Relays { get; set; }
        public System.Data.Entity.DbSet<Sensor> Sensors { get; set; }


        public DbContext() : base("GhAPIAzureContext")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ControlHistory>().HasKey(ct => new { ct.ControllableID, ct.DateTime});
            modelBuilder.Entity<Greenhouse>().HasRequired(gh => gh.Person)
                .WithMany(p => p.Greenhouses).HasForeignKey(gh=>gh.PersonId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasKey(p => p.ID)
                .Property(p => p.ID)
                 .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Controllable>().HasOptional(ct => ct.Relay)
                .WithOptionalDependent(r => r.Controlable)
                .WillCascadeOnDelete(false);

            // modelBuilder.Entity<SensorData>().HasKey(sd => new { sd.DateTime});

            modelBuilder.Entity<SensorData>().HasKey(sd => new { sd.SensorID, sd.DateTime })
                .Property(sd => sd.GreenhouseID).IsOptional();                
            modelBuilder.Entity<Greenhouse>().HasMany(gh => gh.SensorData)
                .WithOptional(sd => sd.Greenhouse)
                .WillCascadeOnDelete(false);

            // modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

    }
}

