using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Quickbird.Models;

namespace Quickbird.Migrations
{
    [DbContext(typeof(QbDbContext))]
    [Migration("20161103141839_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("Qb.Poco.Global.CropType", b =>
                {
                    b.Property<string>("Name");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<Guid?>("CreatedBy");

                    b.Property<bool>("Deleted");

                    b.HasKey("Name");

                    b.ToTable("CropTypes");
                });

            modelBuilder.Entity("Qb.Poco.Global.Parameter", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Unit");

                    b.HasKey("Id");

                    b.ToTable("Parameters");
                });

            modelBuilder.Entity("Qb.Poco.Global.Placement", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Placements");
                });

            modelBuilder.Entity("Qb.Poco.Global.SensorType", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ParameterId");

                    b.Property<long>("PlacementId");

                    b.Property<long>("SubsystemId");

                    b.HasKey("Id");

                    b.HasIndex("ParameterId");

                    b.HasIndex("PlacementId");

                    b.HasIndex("SubsystemId");

                    b.ToTable("SensorTypes");
                });

            modelBuilder.Entity("Qb.Poco.Global.Subsystem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Subsystems");
                });

            modelBuilder.Entity("Qb.Poco.Person", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("People");
                });

            modelBuilder.Entity("Qb.Poco.User.CropCycle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("CropTypeName");

                    b.Property<bool>("Deleted");

                    b.Property<DateTimeOffset?>("EndDate");

                    b.Property<Guid>("LocationId");

                    b.Property<string>("Name");

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<double>("Yield");

                    b.HasKey("Id");

                    b.HasIndex("CropTypeName");

                    b.HasIndex("LocationId");

                    b.ToTable("CropCycles");
                });

            modelBuilder.Entity("Qb.Poco.User.Device", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("LocationId");

                    b.Property<string>("Name");

                    b.Property<Guid>("SerialNumber");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("LocationId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Qb.Poco.User.Location", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name");

                    b.Property<Guid?>("PersonId");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("PersonId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("Qb.Poco.User.Sensor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("DeviceId");

                    b.Property<bool>("Enabled");

                    b.Property<long>("SensorTypeId");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("SensorTypeId");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("Qb.Poco.User.SensorHistory", b =>
                {
                    b.Property<Guid>("SensorId");

                    b.Property<DateTime>("UtcDate");

                    b.Property<Guid>("LocationId");

                    b.Property<byte[]>("RawData");

                    b.Property<DateTimeOffset>("UploadedAt");

                    b.HasKey("SensorId", "UtcDate");

                    b.HasIndex("LocationId");

                    b.HasIndex("SensorId");

                    b.ToTable("SensorsHistory");
                });

            modelBuilder.Entity("Qb.Poco.Global.SensorType", b =>
                {
                    b.HasOne("Qb.Poco.Global.Parameter", "Parameter")
                        .WithMany("SensorTypes")
                        .HasForeignKey("ParameterId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Qb.Poco.Global.Placement", "Placement")
                        .WithMany("SensorTypes")
                        .HasForeignKey("PlacementId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Qb.Poco.Global.Subsystem", "Subsystem")
                        .WithMany("SensorTypes")
                        .HasForeignKey("SubsystemId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Qb.Poco.User.CropCycle", b =>
                {
                    b.HasOne("Qb.Poco.Global.CropType", "CropType")
                        .WithMany("CropCycles")
                        .HasForeignKey("CropTypeName");

                    b.HasOne("Qb.Poco.User.Location", "Location")
                        .WithMany("CropCycles")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Qb.Poco.User.Device", b =>
                {
                    b.HasOne("Qb.Poco.User.Location", "Location")
                        .WithMany("Devices")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Qb.Poco.User.Location", b =>
                {
                    b.HasOne("Qb.Poco.Person", "Person")
                        .WithMany("Locations")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("Qb.Poco.User.Sensor", b =>
                {
                    b.HasOne("Qb.Poco.User.Device", "Device")
                        .WithMany("Sensors")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Qb.Poco.Global.SensorType", "SensorType")
                        .WithMany()
                        .HasForeignKey("SensorTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Qb.Poco.User.SensorHistory", b =>
                {
                    b.HasOne("Qb.Poco.User.Location", "Location")
                        .WithMany("SensorHistories")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Qb.Poco.User.Sensor", "Sensor")
                        .WithMany("SensorHistories")
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
