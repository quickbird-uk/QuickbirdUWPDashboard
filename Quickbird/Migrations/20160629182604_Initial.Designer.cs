using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Quickbird.Models;

namespace Quickbird.Migrations
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20160629182604_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("DbStructure.CropType", b =>
                {
                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 245);

                    b.Property<bool>("Approved");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<Guid?>("CreatedBy");

                    b.HasKey("Name");

                    b.ToTable("CropTypes");
                });

            modelBuilder.Entity("DbStructure.Device", b =>
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

            modelBuilder.Entity("DbStructure.Global.Parameter", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Unit");

                    b.HasKey("ID");

                    b.ToTable("Parameters");
                });

            modelBuilder.Entity("DbStructure.Global.Placement", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Placements");
                });

            modelBuilder.Entity("DbStructure.Global.RelayType", b =>
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

            modelBuilder.Entity("DbStructure.Global.SensorType", b =>
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

            modelBuilder.Entity("DbStructure.Global.Subsystem", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");

                    b.ToTable("Subsystems");
                });

            modelBuilder.Entity("DbStructure.Relay", b =>
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

            modelBuilder.Entity("DbStructure.Sensor", b =>
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

            modelBuilder.Entity("DbStructure.User.CropCycle", b =>
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

            modelBuilder.Entity("DbStructure.User.Location", b =>
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

            modelBuilder.Entity("DbStructure.User.Person", b =>
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

            modelBuilder.Entity("DbStructure.User.RelayHistory", b =>
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

            modelBuilder.Entity("DbStructure.User.SensorHistory", b =>
                {
                    b.Property<Guid>("SensorID");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid?>("LocationID");

                    b.Property<byte[]>("RawData");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.HasKey("SensorID", "TimeStamp");

                    b.HasIndex("LocationID");

                    b.HasIndex("SensorID");

                    b.ToTable("SensorsHistory");
                });

            modelBuilder.Entity("DbStructure.Device", b =>
                {
                    b.HasOne("DbStructure.User.Location", "Location")
                        .WithMany("Devices")
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.Global.RelayType", b =>
                {
                    b.HasOne("DbStructure.Global.Subsystem", "Subsystem")
                        .WithMany("ControlTypes")
                        .HasForeignKey("SubsystemID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.Global.SensorType", b =>
                {
                    b.HasOne("DbStructure.Global.Parameter", "Param")
                        .WithMany()
                        .HasForeignKey("ParamID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DbStructure.Global.Placement", "Place")
                        .WithMany()
                        .HasForeignKey("PlaceID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DbStructure.Global.Subsystem", "Subsystem")
                        .WithMany("SensorTypes")
                        .HasForeignKey("SubsystemID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.Relay", b =>
                {
                    b.HasOne("DbStructure.Device", "Device")
                        .WithMany("Relays")
                        .HasForeignKey("DeviceID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DbStructure.Global.RelayType", "RelayType")
                        .WithMany()
                        .HasForeignKey("RelayTypeID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.Sensor", b =>
                {
                    b.HasOne("DbStructure.Device", "Device")
                        .WithMany("Sensors")
                        .HasForeignKey("DeviceID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DbStructure.Global.Placement")
                        .WithMany("Sensors")
                        .HasForeignKey("PlacementID");

                    b.HasOne("DbStructure.Global.SensorType", "SensorType")
                        .WithMany()
                        .HasForeignKey("SensorTypeID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.User.CropCycle", b =>
                {
                    b.HasOne("DbStructure.CropType", "CropType")
                        .WithMany("CropCycles")
                        .HasForeignKey("CropTypeName")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DbStructure.User.Location", "Location")
                        .WithMany("CropCycles")
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.User.Location", b =>
                {
                    b.HasOne("DbStructure.User.Person", "Person")
                        .WithMany("Locations")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("DbStructure.User.RelayHistory", b =>
                {
                    b.HasOne("DbStructure.User.Location", "Location")
                        .WithMany("RelayHistory")
                        .HasForeignKey("LocationID");

                    b.HasOne("DbStructure.Relay", "Relay")
                        .WithMany("RelayHistory")
                        .HasForeignKey("RelayID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("DbStructure.User.SensorHistory", b =>
                {
                    b.HasOne("DbStructure.User.Location", "Location")
                        .WithMany("SensorHistory")
                        .HasForeignKey("LocationID")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("DbStructure.Sensor", "Sensor")
                        .WithMany("SensorHistory")
                        .HasForeignKey("SensorID")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
