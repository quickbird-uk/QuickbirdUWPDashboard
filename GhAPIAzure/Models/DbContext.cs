using DbStructure;
using DbStructure.Global;
using DbStructure.User;
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
    public class DataContext : System.Data.Entity.DbContext
    {
        public System.Data.Entity.DbSet<RelayType> RelayTypes { get; set; }
        public System.Data.Entity.DbSet<Parameter> Parameters { get; set; }
        public System.Data.Entity.DbSet<Placement> Placements { get; set; }
        public System.Data.Entity.DbSet<SensorType> SensorTypes { get; set; }
        public System.Data.Entity.DbSet<Subsystem> Subsystems { get; set; }

        //User Data
        public System.Data.Entity.DbSet<Person> People { get; set; }
        public System.Data.Entity.DbSet<Location> Location { get; set; }
        public System.Data.Entity.DbSet<CropCycle> CropCycles { get; set; }
        public System.Data.Entity.DbSet<CropType> CropTypes { get; set; }
        public System.Data.Entity.DbSet<RelayHistory> RelayHistories { get; set; }
        public System.Data.Entity.DbSet<SensorHistory> SensorHistories { get; set; }

        //Device Data
        public System.Data.Entity.DbSet<Device> Devices{ get; set; }
        public System.Data.Entity.DbSet<Relay> Relays { get; set; }
        public System.Data.Entity.DbSet<Sensor> Sensors { get; set; }


        public DataContext() : base("GhAPIAzureContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RelayHistory>().HasKey(ct => new { ct.RelayID, ct.TimeStamp});

            modelBuilder.Entity<SensorHistory>().HasKey(sd => new { sd.SensorID, sd.TimeStamp });


            modelBuilder.Entity<Location>().HasRequired(gh => gh.Person)
                .WithMany(p => p.Locations).HasForeignKey(gh=>gh.PersonId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Person>()
                .HasKey(p => p.ID)
                .Property(p => p.ID)
                 .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            //modelBuilder.Entity<Controllable>().HasOptional(ct => ct.Relay)
            //    .WithOptionalDependent(sd => sd.Controlable)
            //    .Map(cr => cr.MapKey("RelayID"))
            //    .WillCascadeOnDelete(false); 




            //modelBuilder.Entity<SensorHistory>().HasKey(sd => new { sd.SensorID, sd.TimeStamp })
            //   .Property(sd => sd.LocationID).IsOptional();

            modelBuilder.Entity<Sensor>().Property(sn => sn.AlertHigh).IsOptional();
            modelBuilder.Entity<Sensor>().Property(sn => sn.AlertLow).IsOptional();


            modelBuilder.Entity<Location>().HasMany(gh => gh.SensorHistory)
                .WithOptional(sd => sd.Location)
                .WillCascadeOnDelete(false);

            //Add index on cropType Name and make it non Db generated
            modelBuilder.Entity<CropType>().HasKey(Ct => Ct.Name)
                .Property(ct => ct.Name)
                 .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<CropType>().HasMany(Ct => Ct.CropCycles)
                .WithRequired(cc => cc.CropType).HasForeignKey(cc => cc.CropTypeName);

            modelBuilder.Entity<RelayHistory>().Ignore(rh => rh.Data);


            modelBuilder.Entity<SensorHistory>().Ignore(sh => sh.Data);
            modelBuilder.Entity<SensorHistory>().Property(sh => sh.UpdatedAt)
                .HasColumnAnnotation(
                IndexAnnotation.AnnotationName,
                new IndexAnnotation(new IndexAttribute()));


            base.OnModelCreating(modelBuilder);
        }

    }
}

