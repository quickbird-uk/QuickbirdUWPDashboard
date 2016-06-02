using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Agronomist.Models;

namespace Agronomist.Migrations
{
    [DbContext(typeof(MainDbContext))]
    partial class MainDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rc2-20896");

            modelBuilder.Entity("DatabasePOCOs.CropType", b =>
                {
                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 245);

                    b.Property<bool>("Approved");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<Guid?>("CreatedBy");

                    b.HasKey("Name");

                    b.ToTable("CropTypes");
                });

            modelBuilder.Entity("DatabasePOCOs.Device", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("LocationID");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid>("SerialNumber");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");

                    b.HasIndex("LocationID");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Parameter", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Unit");

                    b.HasKey("ID");

                    b.ToTable("Parameters");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Placement", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Placements");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.RelayType", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Additive");

                    b.Property<string>("Name");

                    b.Property<long>("SubsystemID");

                    b.HasKey("ID");

                    b.HasIndex("SubsystemID");

                    b.ToTable("RelayTypes");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.SensorType", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ParamID");

                    b.Property<long>("PlaceID");

                    b.Property<long>("SubsystemID");

                    b.HasKey("ID");

                    b.HasIndex("ParamID");

                    b.HasIndex("PlaceID");

                    b.HasIndex("SubsystemID");

                    b.ToTable("SensorTypes");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Subsystem", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Subsystems");
                });

            modelBuilder.Entity("DatabasePOCOs.Relay", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("DeviceID");

                    b.Property<bool>("Enabled");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("OffTime");

                    b.Property<int>("OnTime");

                    b.Property<long>("RelayTypeID");

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");

                    b.HasIndex("DeviceID");

                    b.HasIndex("RelayTypeID");

                    b.ToTable("Relays");
                });

            modelBuilder.Entity("DatabasePOCOs.Sensor", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Alarmed");

                    b.Property<double?>("AlertHigh");

                    b.Property<double?>("AlertLow");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("DeviceID");

                    b.Property<bool>("Enabled");

                    b.Property<double>("Multiplier");

                    b.Property<double>("Offset");

                    b.Property<long?>("PlacementID");

                    b.Property<long>("SensorTypeID");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");

                    b.HasIndex("DeviceID");

                    b.HasIndex("PlacementID");

                    b.HasIndex("SensorTypeID");

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropCycle", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("CropTypeName")
                        .IsRequired();

                    b.Property<string>("CropVariety")
                        .IsRequired();

                    b.Property<bool>("Deleted");

                    b.Property<DateTimeOffset?>("EndDate");

                    b.Property<Guid>("LocationID");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.Property<double>("Yield");

                    b.HasKey("ID");

                    b.HasIndex("CropTypeName");

                    b.HasIndex("LocationID");

                    b.ToTable("CropCycles");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Location", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<Guid>("PersonId");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");

                    b.HasIndex("PersonId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Person", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("TwitterHandle");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<string>("UserName");

                    b.Property<ulong>("twitterID");

                    b.HasKey("ID");

                    b.ToTable("People");
                });

            modelBuilder.Entity("DatabasePOCOs.User.RelayHistory", b =>
                {
                    b.Property<Guid>("RelayID");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid?>("LocationID");

                    b.Property<byte[]>("RawData");

                    b.HasKey("RelayID", "TimeStamp");

                    b.HasIndex("LocationID");

                    b.HasIndex("RelayID");

                    b.ToTable("RelayHistory");
                });

            modelBuilder.Entity("DatabasePOCOs.User.SensorHistory", b =>
                {
                    b.Property<Guid>("SensorID");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid?>("LocationID");

                    b.Property<byte[]>("RawData");

                    b.HasKey("SensorID", "TimeStamp");

                    b.HasIndex("LocationID");

                    b.HasIndex("SensorID");

                    b.ToTable("SensorsHistory");
                });

            modelBuilder.Entity("DatabasePOCOs.Device", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.Global.RelayType", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.Subsystem")
                        .WithMany()
                        .HasForeignKey("SubsystemID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.Global.SensorType", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.Parameter")
                        .WithMany()
                        .HasForeignKey("ParamID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DatabasePOCOs.Global.Placement")
                        .WithMany()
                        .HasForeignKey("PlaceID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DatabasePOCOs.Global.Subsystem")
                        .WithMany()
                        .HasForeignKey("SubsystemID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.Relay", b =>
                {
                    b.HasOne("DatabasePOCOs.Device")
                        .WithMany()
                        .HasForeignKey("DeviceID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DatabasePOCOs.Global.RelayType")
                        .WithMany()
                        .HasForeignKey("RelayTypeID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.Sensor", b =>
                {
                    b.HasOne("DatabasePOCOs.Device")
                        .WithMany()
                        .HasForeignKey("DeviceID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DatabasePOCOs.Global.Placement")
                        .WithMany()
                        .HasForeignKey("PlacementID");

                    b.HasOne("DatabasePOCOs.Global.SensorType")
                        .WithMany()
                        .HasForeignKey("SensorTypeID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropCycle", b =>
                {
                    b.HasOne("DatabasePOCOs.CropType")
                        .WithMany()
                        .HasForeignKey("CropTypeName")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.User.Location", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Person")
                        .WithMany()
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("DatabasePOCOs.User.RelayHistory", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID");

                    b.HasOne("DatabasePOCOs.Relay")
                        .WithMany()
                        .HasForeignKey("RelayID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DatabasePOCOs.User.SensorHistory", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("DatabasePOCOs.Sensor")
                        .WithMany()
                        .HasForeignKey("SensorID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
