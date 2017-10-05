namespace DbStructure.User
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    public class SensorHistory
    {
        //EDIT EF code to make this NOT mapped to a table! Otherwise we will have trouble! 
        //this is used for network communication and by the program at runtime! 
        [Required]
        public virtual List<SensorDatapoint> Data { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }

        public Guid? LocationID { get; set; }

        [JsonIgnore]
        public byte[] RawData { get; set; } = new byte[0];

        [JsonIgnore]
        public virtual Sensor Sensor { get; set; }

        [Required]
        public Guid SensorID { get; set; }

        [Required]
        public DateTimeOffset TimeStamp { get; set; }

        /// <summary>The datetime that this history was uploaded to the server, always set by the server and
        /// only by the server. For the local computer this is a gaurantee that data up to this point has been
        /// downloaded from the server.</summary>
        [Required]
        public DateTimeOffset UploadedAt { get; set; } = default(DateTimeOffset);

        public void DeserialiseData()
        {
            var dataSize = SensorDatapoint.BinarySize;
            var dataItems = new List<SensorDatapoint>();

            for (var i = 0; i < RawData.Length; i += dataSize)
            {
                var value = BitConverter.ToDouble(RawData, i);
                var duration = TimeSpan.FromTicks(BitConverter.ToInt64(RawData, i + 8));
                var timestampTicks = BitConverter.ToInt64(RawData, i + 16);
                //we need to add the time in because the timestampTicks are in UTC
                var timeStamp = new DateTimeOffset(timestampTicks, TimeStamp.Offset).Add(TimeStamp.Offset);
                dataItems.Add(new SensorDatapoint(value, timeStamp, duration));
            }
            Data = dataItems;
        }

        public static SensorHistory Merge(SensorHistory slice1, SensorHistory slice2)
        {
            if (slice1.SensorID != slice2.SensorID)
            {
                throw new Exception("Attempted to merge SensorHistory slices from different sensors! " + slice1.SensorID +
                                    " and " + slice2.SensorID);
            }
            if (slice1.LocationID != slice2.LocationID)
            {
                throw new Exception("Attempted to merge SensorHistory from different Locations!! " + slice1.LocationID +
                                    " and " + slice2.LocationID);
            }
            if ((slice1.TimeStamp - slice2.TimeStamp).TotalHours > 24 ||
                (slice2.TimeStamp - slice1.TimeStamp).TotalHours > 24)
            {
                throw new Exception("Attempted to merge SensorHistory from different days! " + slice1.TimeStamp +
                                    " and " + slice2.TimeStamp);
            }


            var result = new SensorHistory
            {
                SensorID = slice1.SensorID,
                Sensor = slice1.Sensor,
                TimeStamp = slice1.TimeStamp,
                Location = slice1.Location,
                LocationID = slice1.LocationID,
                // Take the smaller uploaded at so all the data after that can be uploaded.
                UploadedAt = slice1.UploadedAt < slice2.UploadedAt ? slice1.UploadedAt : slice2.UploadedAt,
                Data = new List<SensorDatapoint>()
            };

            var tempList = new List<SensorDatapoint>(slice1.Data);
            tempList.AddRange(slice2.Data);

            tempList.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));

            for (var i = 0; i < tempList.Count; i++)
            {
                if (i >= tempList.Count - 1)
                {
                    result.Data.Add(tempList[i]);
                }
                else if (tempList[i].TimeStamp != tempList[i + 1].TimeStamp)
                {
                    result.Data.Add(tempList[i]);
                }
            }
            return result;
        }

        public void SerialiseData()
        {
            var dataSize = SensorDatapoint.BinarySize;
            if (Data == null)
                return; 

            var dataRaw = new byte[Data.Count*dataSize];

            for (var i = 0; i < Data.Count; i++)
            {
                var valueBytes = BitConverter.GetBytes(Data[i].Value);
                Array.Copy(valueBytes, 0, dataRaw, i*dataSize, 8);
                var durationBytes = BitConverter.GetBytes(Data[i].Duration.Ticks);
                Array.Copy(durationBytes, 0, dataRaw, i*dataSize + 8, 8);
                var ticks = Data[i].TimeStamp.UtcTicks;
                var timestampBytes = BitConverter.GetBytes(ticks);
                Array.Copy(timestampBytes, 0, dataRaw, i*dataSize + 16, 8);
            }
            RawData = dataRaw;
        }

        /// <summary>This methos slices the SensorHistory item in two, and produces an entirely new item which
        /// only includes time stamps taken after time requested.</summary>
        /// <param name="slicePoint">Datapoints before this time are removed</param>
        /// <returns></returns>
        public SensorHistory Slice(DateTimeOffset slicePoint)
        {
            var result = new SensorHistory
            {
                Sensor = Sensor,
                SensorID = SensorID,
                TimeStamp = TimeStamp,
                Location = Location,
                LocationID = LocationID,
                UploadedAt = UploadedAt,
                Data = new List<SensorDatapoint>()
            };
            foreach (var item in Data)
            {
                if (item.TimeStamp > slicePoint)
                {
                    result.Data.Add(item);
                }
            }
            return result;
        }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //if (TimeStamp == null)
            //{
            //    yield return new ValidationResult("You need to provide a timestamp");
            //};
            if (TimeStamp.TimeOfDay != TimeSpan.Zero)
            {
                yield return new ValidationResult("Timestamp on the Day should be zero!");
            }
            ;
            if (Data.Exists(dt => dt.TimeStamp > TimeStamp))
            {
                yield return new ValidationResult("Timestamps of measurements must be earlier than the day's Timestamp")
                    ;
            }
            ;
            if (Data.Exists(dt => (TimeStamp - dt.TimeStamp).TotalHours > 24))
            {
                yield return new ValidationResult("Timestamps should be taken withing 24 hours of the day.");
            }
            ;
            if (Data.Exists(dt => dt.TimeStamp > DateTimeOffset.Now))
            {
                yield return new ValidationResult("You can't create a datapoint in the future");
            }
            ;
            if (TimeStamp > DateTimeOffset.Now.AddDays(1))
            {
                yield return new ValidationResult("You can't create a day that's more than 24 hours in the future");
            }
            ;
        }
    }


    public class SensorDatapoint
    {
        /// <summary>Size of these structures in bytes</summary>
        public static readonly int BinarySize = 24;

        [Required] public readonly TimeSpan Duration;

        [Required] public readonly DateTimeOffset TimeStamp;

        [Required] public readonly double Value;

        //public override bool Equals(object obj)
        //{
        //    SensorDatapoint comparand = obj as SensorDatapoint;
        //    if (comparand == null)
        //        return false;
        //    else 
        //    return base.Equals(obj);
        //}

        public SensorDatapoint(double value, DateTimeOffset timestamp, TimeSpan duration)
        {
            Value = value;
            TimeStamp = timestamp;
            Duration = duration;
        }
    }
}
