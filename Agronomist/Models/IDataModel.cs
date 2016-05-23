namespace Agronomist.Models
{
    using System;
    using System.Threading.Tasks;
    using DatabasePOCOs;
    using DatabasePOCOs.Global;
    using DatabasePOCOs.User;
    using Microsoft.EntityFrameworkCore;
    using NetLib;

    public interface IDataModel
    {
        DbSet<CropCycle> CropCycles { get; }
        DbSet<CropType> CropTypes { get; }
        DbSet<Device> Devices { get; }
        DbSet<Location> Locations { get; }
        DbSet<Parameter> Parameters { get; }
        DbSet<Person> People { get; }
        DbSet<Placement> Placements { get; }
        DbSet<RelayHistory> RelayHistory { get; }
        DbSet<Relay> Relays { get; }
        DbSet<RelayType> RelayTypes { get; }
        DbSet<SensorHistory> SensorHistory { get; }
        DbSet<Sensor> Sensors { get; }
        DbSet<SensorType> SensorTypes { get; }
        DbSet<Subsystem> Subsystems { get; }

        Task<string> UpdateFromServer(DateTimeOffset lastUpdate, Creds creds);
    }
}