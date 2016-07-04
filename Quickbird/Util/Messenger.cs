namespace Quickbird.Util
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     A middleman class for passing information between parts of the program.
    /// </summary>
    public class Messenger
    {
        public BroadcastMessage<string> NewDeviceDetected { get; } = new BroadcastMessage<string>();

        public BroadcastMessage<IEnumerable<SensorReading>> NewSensorDataPoint { get; } =
            new BroadcastMessage<IEnumerable<SensorReading>>();

        public BroadcastMessage<IEnumerable<RelayReading>> NewRelayDataPoint { get; } =
            new BroadcastMessage<IEnumerable<RelayReading>>();

        public BroadcastMessage<string> TablesChanged { get; } =
            new BroadcastMessage<string>();

        /// <summary>
        ///     App suspending.
        /// </summary>
        public BroadcastMessage<TaskCompletionSource<object>> Suspending { get; } =
            new BroadcastMessage<TaskCompletionSource<object>>();

        /// <summary>
        ///     App resuming.
        /// </summary>
        public BroadcastMessage<TaskCompletionSource<object>> Resuming { get; } =
            new BroadcastMessage<TaskCompletionSource<object>>();

        public BroadcastMessage<string> LocalNetworkConflict { get; } = new BroadcastMessage<string>();

        public BroadcastMessage<object> DeviceManagementEnableChanged { get; } = new BroadcastMessage<object>();

        public struct SensorReading
        {
            public SensorReading(Guid id, double value, DateTimeOffset timestamp, TimeSpan duration)
            {
                Value = value;
                Duration = duration;
                Timestamp = timestamp;
                SensorId = id;
            }

            public double Value { get; }

            public TimeSpan Duration { get; }

            public DateTimeOffset Timestamp { get; }

            public Guid SensorId { get; }
        }

        public struct RelayReading
        {
            public RelayReading(Guid id, bool state, DateTimeOffset timestamp, TimeSpan duration)
            {
                State = state;
                Duration = duration;
                Timestamp = timestamp;
                RelayId = id;
            }

            public bool State { get; }

            public TimeSpan Duration { get; }

            public DateTimeOffset Timestamp { get; }

            public Guid RelayId { get; }
        }

        #region SingletonInit

        private Messenger()
        {
        }

        public static Messenger Instance { get; } = new Messenger();

        #endregion
    }
}