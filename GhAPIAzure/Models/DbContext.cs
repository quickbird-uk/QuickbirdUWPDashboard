namespace GhAPIAzure.Models
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure.Annotations;
    using DbStructure;
    using DbStructure.Global;
    using DbStructure.User;

    public class DataContext : DbContext
    {
        public DataContext() : base("GhAPIAzureContext") { Configuration.LazyLoadingEnabled = false; }

        public DbSet<CropCycle> CropCycles { get; set; }
        public DbSet<CropType> CropTypes { get; set; }

        //Device Data
        public DbSet<Device> Devices { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Parameter> Parameters { get; set; }

        //User Data
        public DbSet<Person> People { get; set; }
        public DbSet<Placement> Placements { get; set; }
        public DbSet<RelayHistory> RelayHistories { get; set; }
        public DbSet<Relay> Relays { get; set; }
        public DbSet<RelayType> RelayTypes { get; set; }
        public DbSet<SensorHistory> SensorHistories { get; set; }
        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<SensorType> SensorTypes { get; set; }
        public DbSet<Subsystem> Subsystems { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RelayHistory>().HasKey(ct => new {ct.RelayID, ct.TimeStamp});

            modelBuilder.Entity<SensorHistory>().HasKey(sd => new {sd.SensorID, sd.TimeStamp});


            modelBuilder.Entity<Location>()
                .HasRequired(gh => gh.Person)
                .WithMany(p => p.Locations)
                .HasForeignKey(gh => gh.PersonId)
                .WillCascadeOnDelete(false);

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


            modelBuilder.Entity<Location>()
                .HasMany(gh => gh.SensorHistory)
                .WithOptional(sd => sd.Location)
                .WillCascadeOnDelete(false);

            //Add index on cropType Name and make it non Db generated
            modelBuilder.Entity<CropType>()
                .HasKey(Ct => Ct.Name)
                .Property(ct => ct.Name)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<CropType>()
                .HasMany(Ct => Ct.CropCycles)
                .WithRequired(cc => cc.CropType)
                .HasForeignKey(cc => cc.CropTypeName);

            modelBuilder.Entity<RelayHistory>().Ignore(rh => rh.Data);


            modelBuilder.Entity<SensorHistory>().Ignore(sh => sh.Data);
            modelBuilder.Entity<SensorHistory>()
                .Property(sh => sh.UploadedAt)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute()));


            base.OnModelCreating(modelBuilder);
        }
    }
}
