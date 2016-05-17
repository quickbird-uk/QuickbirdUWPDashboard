namespace DatabasePOCOs.User
{
    using Newtonsoft.Json;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class RelayHistory : BaseHistorical<Relay>, IValidatableObject
    {
        [JsonIgnore]
        public Relay Relay { get; set; }

        [Required]
        public Guid RelayID { get; set; }

        //EDIT EF code to make this NOT mapped to a table! Otherwise we will have trouble! 
        //this is used for network communication and by the program at runtime! 
        [Required]
        public List<RelayDatapoint> Data { get; set; }

        public override object Thing
        {
            get { return Relay; }
            set { Relay = (Relay) value; }
        }

        public override Guid ThingID
        {
            get { return RelayID; }
            set { RelayID = value; }
        }
        
        public override IList ThingData
        {
            get { return Data; }
            set { Data = (List<RelayDatapoint>) value; }
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