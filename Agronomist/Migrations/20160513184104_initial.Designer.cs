using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Agronomist.Models;

namespace Agronomist.Migrations
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20160513184104_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("DatabasePOCOs.CropType", b =>
                {
                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 245);

                    b.Property<bool>("Approved");

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<Guid?>("CreatedBy");

                    b.HasKey("Name");
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
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Parameter", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("Unit");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.Placement", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.RelayType", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Additive");

                    b.Property<string>("Name");

                    b.Property<long>("SubsystemID");

                    b.HasKey("ID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.SensorType", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ParamID");

                    b.Property<long>("PlaceID");

                    b.Property<long>("SubsystemID");

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
                });

            modelBuilder.Entity("DatabasePOCOs.Sensor", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<double?>("AlertHigh")
                        .IsRequired();

                    b.Property<double?>("AlertLow")
                        .IsRequired();

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
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropCycle", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("CreatedAt");

                    b.Property<string>("CropTypeName")
                        .IsRequired();

                    b.Property<bool>("Deleted");

                    b.Property<DateTimeOffset?>("EndDate");

                    b.Property<Guid>("LocationID");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<DateTimeOffset>("StartDate");

                    b.Property<DateTimeOffset>("UpdatedAt");

                    b.Property<byte[]>("Version");

                    b.HasKey("ID");
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
                });

            modelBuilder.Entity("DatabasePOCOs.User.RelayHistory", b =>
                {
                    b.Property<Guid>("RelayID");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid?>("LocationID");

                    b.Property<byte[]>("RawData");

                    b.HasKey("RelayID", "TimeStamp");
                });

            modelBuilder.Entity("DatabasePOCOs.User.SensorHistory", b =>
                {
                    b.Property<Guid>("SensorID");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid?>("LocationID");

                    b.Property<byte[]>("RawData");

                    b.HasKey("SensorID", "TimeStamp");
                });

            modelBuilder.Entity("DatabasePOCOs.Device", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.RelayType", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.Subsystem")
                        .WithMany()
                        .HasForeignKey("SubsystemID");
                });

            modelBuilder.Entity("DatabasePOCOs.Global.SensorType", b =>
                {
                    b.HasOne("DatabasePOCOs.Global.Parameter")
                        .WithMany()
                        .HasForeignKey("ParamID");

                    b.HasOne("DatabasePOCOs.Global.Placement")
                        .WithMany()
                        .HasForeignKey("PlaceID");

                    b.HasOne("DatabasePOCOs.Global.Subsystem")
                        .WithMany()
                        .HasForeignKey("SubsystemID");
                });

            modelBuilder.Entity("DatabasePOCOs.Relay", b =>
                {
                    b.HasOne("DatabasePOCOs.Device")
                        .WithMany()
                        .HasForeignKey("DeviceID");

                    b.HasOne("DatabasePOCOs.Global.RelayType")
                        .WithMany()
                        .HasForeignKey("RelayTypeID");
                });

            modelBuilder.Entity("DatabasePOCOs.Sensor", b =>
                {
                    b.HasOne("DatabasePOCOs.Device")
                        .WithMany()
                        .HasForeignKey("DeviceID");

                    b.HasOne("DatabasePOCOs.Global.Placement")
                        .WithMany()
                        .HasForeignKey("PlacementID");

                    b.HasOne("DatabasePOCOs.Global.SensorType")
                        .WithMany()
                        .HasForeignKey("SensorTypeID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.CropCycle", b =>
                {
                    b.HasOne("DatabasePOCOs.CropType")
                        .WithMany()
                        .HasForeignKey("CropTypeName");

                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.Location", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Person")
                        .WithMany()
                        .HasForeignKey("PersonId");
                });

            modelBuilder.Entity("DatabasePOCOs.User.RelayHistory", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID");

                    b.HasOne("DatabasePOCOs.Relay")
                        .WithMany()
                        .HasForeignKey("RelayID");
                });

            modelBuilder.Entity("DatabasePOCOs.User.SensorHistory", b =>
                {
                    b.HasOne("DatabasePOCOs.User.Location")
                        .WithMany()
                        .HasForeignKey("LocationID");

                    b.HasOne("DatabasePOCOs.Sensor")
                        .WithMany()
                        .HasForeignKey("SensorID");
                });
        }
    }
}
