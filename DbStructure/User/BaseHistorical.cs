using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePOCOs.User
{
    using System.Collections;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public abstract class BaseHistorical<T> : IHistorical
    {

        public abstract object Thing { get; set; }

        public abstract Guid ThingID { get; set; }

        [JsonIgnore]
        public Location Location { get; set; }

        public Guid? LocationID { get; set; }

        [Required]
        public DateTimeOffset TimeStamp { get; set; }

        [JsonIgnore]
        public byte[] RawData { get; set; }

        public abstract IList ThingData { get; set; }

        public abstract void SerialiseData();

        public abstract void DeserialiseData();

        /// <summary>
        /// Creates a new controlHistory object that contains only data past a certain point. 
        /// This is done based on the list, not Raw data. If the list is empty, you will get nothing! 
        /// </summary>
        /// <param name="slicePoint">Time, before which data is not included</param>
        /// <returns></returns>
        public abstract BaseHistorical<T> Slice(DateTimeOffset slicePoint);

        public static T Merge(T slice1, T slice2)
        {
            if (slice1.RelayID != slice2.RelayID)
            {
                throw new Exception("Attempted to merge RelayHistory slices with different ID's! "
                    + slice1.RelayID + " and " + slice2.RelayID);
            }
            if (slice1.LocationID != slice2.LocationID)
            {
                throw new Exception("Attempted to merge RelayHistory from different Locations!! "
                + slice1.LocationID + " and " + slice2.LocationID);
            }
            if ((slice1.TimeStamp - slice2.TimeStamp).TotalHours > 24
                || (slice2.TimeStamp - slice1.TimeStamp).TotalHours > 24)
            {
                throw new Exception("Attempted to merge RelayHistory from different days! "
                   + slice1.TimeStamp + " and " + slice2.TimeStamp);
            }

            RelayHistory result = new RelayHistory
            {
                RelayID = slice1.RelayID,
                Relay = slice1.Relay,
                TimeStamp = slice1.TimeStamp,
                LocationID = slice1.LocationID,
                Location = slice1.Location,
                Data = new List<RelayDatapoint>()
            };
        }
}
