namespace DatabasePOCOs.User
{
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class SensorHistory : BaseHistorical<Sensor>
    {
        [JsonIgnore]
        public virtual Sensor Sensor { get; set; }
        [Required]
        public Guid SensorID { get; set; }

        public override object Thing { get { return Sensor; } set { Sensor = (Sensor) value; } }
        public override Guid ThingID { get { return SensorID; } set { SensorID = value; } }

        public override IList ThingData { get { return Data; } set { Data = (List<SensorDatapoint>) value; } }

        //EDIT EF code to make this NOT mapped to a table! Otherwise we will have trouble! 
        //this is used for network communication and by the program at runtime! 
        [Required]
        public List<SensorDatapoint> Data { get; set; }

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