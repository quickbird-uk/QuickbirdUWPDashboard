namespace DatabasePOCOs.User
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class SensorHistory
    {
        [Required]
        public DateTimeOffset TimeStamp { get; set; }
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; }
        [Required]
        public Guid SensorID { get; set; }
        [JsonIgnore]
        public virtual Location Location { get; set; }
        public Guid? LocationID { get; set; }

        [JsonIgnore]
        public byte[] RawData { get; set; } = new byte[0]; 

        //EDIT EF code to make this NOT mapped to a table! Otherwise we will have trouble! 
        //this is used for network communication and by the program at runtime! 
        [Required]
        public List<SensorDatapoint> Data { get; set; }

        public void SerialiseData()
        {
            int dataSize = SensorDatapoint.BinarySize;
            byte[] dataRaw = new byte[Data.Count * dataSize];

            for(int i=0; i < Data.Count; i++)
            {
                byte[] valueBytes = BitConverter.GetBytes(Data[i].Value);
                Array.Copy(valueBytes, 0, dataRaw, i * dataSize, 8);
                byte[] durationBytes = BitConverter.GetBytes(Data[i].Duration.Ticks);
                Array.Copy(durationBytes, 0, dataRaw, i * dataSize + 8, 8);
                long ticks = Data[i].TimeStamp.UtcTicks;
                byte[] timestampBytes = BitConverter.GetBytes(ticks);
                Array.Copy(timestampBytes, 0, dataRaw, i * dataSize + 16, 8);
            }
            RawData = dataRaw; 
        }

        public void DeserialiseData()
        {
            int dataSize = SensorDatapoint.BinarySize;
            List<SensorDatapoint> dataItems = new List<SensorDatapoint>();

            for(int i=0; i < RawData.Length; i += dataSize)
            {
                double value = BitConverter.ToDouble(RawData, i);
                TimeSpan duration = TimeSpan.FromTicks(BitConverter.ToInt64(RawData, i + 8));
                long timestampTicks = BitConverter.ToInt64(RawData, i + 16);
                DateTimeOffset timeStamp = new DateTimeOffset(timestampTicks, TimeStamp.Offset);
                dataItems.Add(new SensorDatapoint(value, timeStamp, duration));
            }
            Data = dataItems;
        }

        /// <summary>
        /// This methos slices the SensorHistory item in two, and produces an entirely new item which only includes 
        /// time stamps taken after time requested.
        /// </summary>
        /// <param name="slicePoint">Datapoints before this time are removed</param>
        /// <returns></returns>
        public SensorHistory Slice(DateTimeOffset slicePoint)
        {
            SensorHistory result = new SensorHistory
            {
                Sensor = this.Sensor,
                SensorID = this.SensorID,
                TimeStamp = this.TimeStamp,
                Location = this.Location,
                LocationID = this.LocationID,
                Data = new List<SensorDatapoint>()
            };
            foreach(SensorDatapoint item in Data)
            {
                if(item.TimeStamp > slicePoint)
                {
                    result.Data.Add(item); 
                }
            }
            return result; 
        }

        public static SensorHistory Merge(SensorHistory slice1, SensorHistory slice2)
        {

            if (slice1.SensorID!= slice2.SensorID)
            {
                throw new Exception("Attempted to merge SensorHistory slices from different sensors! "
                + slice1.SensorID + " and " + slice2.SensorID);
            }
            if (slice1.LocationID != slice2.LocationID)
            {
                throw new Exception("Attempted to merge SensorHistory from different Locations!! "
                + slice1.LocationID + " and " + slice2.LocationID);
            }
            if ((slice1.TimeStamp - slice2.TimeStamp).TotalHours > 24
            || (slice2.TimeStamp - slice1.TimeStamp).TotalHours > 24)
            {
                throw new Exception("Attempted to merge SensorHistory from different days! "
                    + slice1.TimeStamp + " and " + slice2.TimeStamp);
            }



            SensorHistory result = new SensorHistory
            {
                SensorID = slice1.SensorID,
                Sensor = slice1.Sensor,
                TimeStamp = slice1.TimeStamp,
                Location = slice1.Location,
                LocationID = slice1.LocationID,
                Data = new List<SensorDatapoint>()
            };

            List<SensorDatapoint> tempList = new List<SensorDatapoint>(slice1.Data);
            tempList.AddRange(slice2.Data);

            tempList.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));

            for (int i = 0; i < tempList.Count; i++)
            {
                if (i >= (tempList.Count - 1))
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


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //if (TimeStamp == null)
            //{
            //    yield return new ValidationResult("You need to provide a timestamp");
            //};
            if (TimeStamp.TimeOfDay != TimeSpan.Zero)
            {
                yield return new ValidationResult("Timestamp on the Day should be zero!");
            };
            if (Data.Exists(dt => dt.TimeStamp > TimeStamp))
            {
                yield return new ValidationResult("Timestamps of measurements must be earlier than the day's Timestamp");
            };
            if (Data.Exists(dt => (TimeStamp - dt.TimeStamp).TotalHours > 24))
            {
                yield return new ValidationResult("Timestamps should be taken withing 24 hours of the day.");
            };
            if (Data.Exists(dt => dt.TimeStamp > DateTimeOffset.Now))
            {
                yield return new ValidationResult("You can't create a datapoint in the future");
            };
            if (TimeStamp > DateTimeOffset.Now.AddDays(1))
            {
                yield return new ValidationResult("You can't create a day that's more than 24 hours in the future");
            };
        }
    }



    public class SensorDatapoint
    {
        [Required]
        public readonly double Value;
        [Required]
        public readonly DateTimeOffset TimeStamp;
        [Required]
        public readonly TimeSpan Duration;

        public SensorDatapoint(double value, DateTimeOffset timestamp, TimeSpan duration)
        {
            Value = value;
            TimeStamp = timestamp;
            Duration = duration;
        }
        /// <summary>
        /// Size of these structures in bytes
        /// </summary>
        public static readonly int BinarySize = 24;
    }
}