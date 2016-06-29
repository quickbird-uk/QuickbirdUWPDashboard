namespace DbStructure.User
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RelayHistory : IValidatableObject
    {
        [JsonIgnore]
        public virtual Relay Relay { get; set; }

        [Required]
        public Guid RelayID { get; set; }

        [JsonIgnore]
        public virtual Location Location { get; set; }

        public Guid? LocationID { get; set; }

        [Required]
        public DateTimeOffset TimeStamp { get; set; }

        [JsonIgnore]
        public byte[] RawData { get; set; } = new byte[0];

        //EDIT EF code to make this NOT mapped to a table! Otherwise we will have trouble! 
        //this is used for network communication and by the program at runtime! 
        [Required]
        public List<RelayDatapoint> Data { get; set; }




        public void SerialiseData()
        {
            int dataSize = RelayDatapoint.BinarySize; 
            byte[] dataRaw = new byte[Data.Count * dataSize];

            for (int i = 0; i < Data.Count; i++)
            {
                dataRaw[i * dataSize] = Data[i].State ? (byte)1 : (byte)0;
                byte[] durationBytes = BitConverter.GetBytes(Data[i].Duration.Ticks);
                Array.Copy(durationBytes, 0, dataRaw, i * dataSize + 1, 8);
                long ticks = Data[i].TimeStamp.UtcTicks;
                byte[] timestampBytes = BitConverter.GetBytes(ticks);
                Array.Copy(timestampBytes, 0, dataRaw, i * dataSize + 9, 8);
            }
            RawData = dataRaw;
        }

        public void DeserialiseData()
        {
            int dataSize = RelayDatapoint.BinarySize;
            List<RelayDatapoint> dataItems = new List<RelayDatapoint>();
            
            for(int i=0; i < RawData.Length; i +=dataSize)
            {

                bool state = RawData[i] > 0;
                TimeSpan duration = TimeSpan.FromTicks(BitConverter.ToInt64(RawData, i + 1));
                long timestampTicks = BitConverter.ToInt64(RawData, i + 9);
                DateTimeOffset timeStamp = new DateTimeOffset(timestampTicks, TimeStamp.Offset).Add(TimeStamp.Offset); ;
                dataItems.Add(new RelayDatapoint(state, timeStamp, duration)); 
            }

            Data = dataItems; 
        }

        /// <summary>
        /// Creates a new controlHistory object that contains only data past a certain point. 
        /// This is done based on the list, not Raw data. If the list is empty, you will get nothing! 
        /// </summary>
        /// <param name="slicePoint">Time, before which data is not included</param>
        /// <returns></returns>
        public RelayHistory Slice(DateTimeOffset slicePoint)
        {
            RelayHistory result = new RelayHistory
            {
                Relay = this.Relay,
                RelayID = this.RelayID,
                TimeStamp = this.TimeStamp,
                LocationID = this.LocationID,
                Location = this.Location,
                Data = new List<RelayDatapoint>()
            };
            foreach(RelayDatapoint item in Data)
            {
                if(item.TimeStamp > slicePoint)
                {
                    result.Data.Add(item); 
                }
            }
            return result; 
        }

        /// <summary>
        /// Will only merge the two if they have same RelayID and same Timestamp. 
        /// Mergning is done based on the list, not raw data.
        /// </summary>
        /// <param name="mergeWith"></param>
        /// <returns></returns>
        public static RelayHistory Merge(RelayHistory slice1, RelayHistory slice2)
        {
            if(slice1.RelayID != slice2.RelayID)
            {
                throw new Exception("Attempted to merge RelayHistory slices with different ID's! "
                    + slice1.RelayID + " and " + slice2.RelayID);
            }
            if(slice1.LocationID != slice2.LocationID)
            {
                throw new Exception("Attempted to merge RelayHistory from different Locations!! "
                + slice1.LocationID + " and " + slice2.LocationID);
            }
            if((slice1.TimeStamp - slice2.TimeStamp).TotalHours > 24
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

            List<RelayDatapoint> tempList = new List<RelayDatapoint>(slice1.Data);
            tempList.AddRange(slice2.Data); 

            tempList.Sort((a, b) => a.TimeStamp.CompareTo(b.TimeStamp));
            
            for(int i =0; i< tempList.Count; i++ )
            {
                if(i >= (tempList.Count - 1))
                {
                    result.Data.Add(tempList[i]);
                }
                else if(tempList[i].TimeStamp != tempList[i+1].TimeStamp)
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

    public class RelayDatapoint
    {
        [Required]
        public readonly bool State;
        [Required]
        public readonly DateTimeOffset TimeStamp;
        [Required]
        public readonly TimeSpan Duration;

        public RelayDatapoint(bool state, DateTimeOffset timestamp, TimeSpan duration)
        {
            State = state;
            TimeStamp = timestamp;
            Duration = duration; 
        }
        /// <summary>
        /// Size of these structures in bytes
        /// </summary>
        public static readonly int BinarySize = 17;  
    }
}