namespace Agronomist.Models
{
    using System;
    using System.Threading.Tasks;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.Data.Entity;
    using NetLib;

    public interface IDataModel
    {
        DbSet<CropCycle> CropCycles { get; set; }
        DbSet<CropType> CropTypes { get; set; }
        DbSet<Device> Devices { get; set; }
        DbSet<Location> Locations { get; set; }
        DbSet<Parameter> Parameters { get; set; }
        DbSet<Person> People { get; set; }
        DbSet<Placement> Placements { get; set; }
        DbSet<RelayHistory> RelayHistory { get; set; }
        DbSet<Relay> Relays { get; set; }
        DbSet<RelayType> RelayTypes { get; set; }
        DbSet<SensorHistory> SensorHistory { get; set; }
        DbSet<Sensor> Sensors { get; set; }
        DbSet<SensorType> SensorTypes { get; set; }
        DbSet<Subsystem> Subsystems { get; set; }

        Task<string> UpdateFromServer(DateTimeOffset lastUpdate, Creds creds);
    }
}