using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Agronomist.Models;

namespace Agronomist.Migrations
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20160421194553_160421-FirstMigration")]
    partial class _160421FirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("DatabasePOCOs.Device", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("GreenhouseID");

                    b.Property<string>("Location");

                    b.Property<string>("Name");

                    b.Property<Guid>("SerialNumber");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.ControlType", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Additive");

                    b.Property<string>("Name");

                    b.Property<long>("SubsystemID");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.ParamAtPlace", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ParamID");

                    b.Property<long>("PlaceID");

                    b.Property<long>("SubsystemID");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Parameter", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Unit");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.PlacementType", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Subsystem", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Relay", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ControlableID");

                    b.Property<Guid>("ControllableID");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("DeviceID");

                    b.Property<bool>("Enabled");

                    b.Property<uint>("OffTime");

                    b.Property<uint>("OnTime");

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Sensor", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<double?>("AlertHigh");

                    b.Property<double?>("AlertLow");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("DeviceID");

                    b.Property<double>("Multiplier");

                    b.Property<double>("Offset");

                    b.Property<long>("ParamAtPLaceID");

                    b.Property<long?>("PlacementTypeID");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.ControlHistory", b =>
                {
                    b.Property<Guid>("ControllableID");

                    b.Property<DateTimeOffset>("DateTime");

                    b.Property<Guid>("Controllable");

                    b.Property<byte[]>("DataDay");

                    b.HasKey("ControllableID", "DateTime");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Controllable", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ControlTypeID");

                    b.Property<long?>("ControlTypeID1");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<Guid>("GreenhouseID");

                    b.Property<string>("Name");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropCycle", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<Guid>("CropTypeID");

                    b.Property<bool>("Deleted");

                    b.Property<DateTimeOffset?>("EndDate");

                    b.Property<Guid>("GreenhouseID");

                    b.Property<string>("Name");

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropType", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Greenhouse", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<bool>("Deleted");

                    b.Property<string>("Name");

                    b.Property<long>("PersonId");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Person", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("TwitterHandle");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<string>("UserName");

                    b.Property<ulong>("twitterID");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.SensorData", b =>
                {
                    b.Property<Guid>("SensorID");

                    b.Property<DateTimeOffset>("DateTime");

                    b.Property<byte[]>("DayData");

                    b.Property<Guid?>("GreenhouseID");

                    b.HasKey("SensorID", "DateTime");
                });

            modelBuilder.Entity("DatabasePOCOs.Device", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Greenhouse")
                        .WithMany()
                        .HasForeignKey("GreenhouseID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.ControlType", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.Subsystem")
                        .WithMany()
                        .HasForeignKey("SubsystemID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.ParamAtPlace", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.Parameter")
                        .WithMany()
                        .HasForeignKey("ParamID");

                    b.HasOne("DatabasePOCOs.Global.PlacementType")
                        .WithMany()
                        .HasForeignKey("PlaceID");

                    b.HasOne("DatabasePOCOs.Global.Subsystem")
                        .WithMany()
                        .HasForeignKey("SubsystemID");
                });

            modelBuilder.Entity("DatabasePOCOs.Relay", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Controllable")
                        .WithOne()
                        .HasForeignKey("DatabasePOCOs.Relay", "ControlableID");

                    b.HasOne("DatabasePOCOs.Device")
                        .WithMany()
                        .HasForeignKey("DeviceID");
                });

            modelBuilder.Entity("DatabasePOCOs.Sensor", b =>
                {
                    b.HasOne("DatabasePOCOs.Device")
                        .WithMany()
                        .HasForeignKey("DeviceID");

                    b.HasOne("DatabasePOCOs.Global.ParamAtPlace")
                        .WithMany()
                        .HasForeignKey("ParamAtPLaceID");

                    b.HasOne("DatabasePOCOs.Global.PlacementType")
                        .WithMany()
                        .HasForeignKey("PlacementTypeID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.ControlHistory", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Controllable")
                        .WithMany()
                        .HasForeignKey("ControllableID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Controllable", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.ControlType")
                        .WithMany()
                        .HasForeignKey("ControlTypeID1");

                    b.HasOne("DatabasePOCOs.User.Greenhouse")
                        .WithMany()
                        .HasForeignKey("GreenhouseID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropCycle", b =>
                {
                    b.HasOne("DatabasePOCOs.User.CropType")
                        .WithMany()
                        .HasForeignKey("CropTypeID");

                    b.HasOne("DatabasePOCOs.User.Greenhouse")
                        .WithMany()
                        .HasForeignKey("GreenhouseID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Greenhouse", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Person")
                        .WithMany()
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("DatabasePOCOs.User.SensorData", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Greenhouse")
                        .WithMany()
                        .HasForeignKey("GreenhouseID");

                    b.HasOne("DatabasePOCOs.Sensor")
                        .WithMany()
                        .HasForeignKey("SensorID");
                });
        }
    }
}
