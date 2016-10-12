using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Qb.Poco.User
{
    /// <summary>The data of a single sensor at a single time.</summary>
    /// <remarks>Not a db class.</remarks>
    public class SensorDatapoint
    {
        /// <summary>The byte length of a single data point when serialised (8:value, 8:duration, 8+8:DateTimeOffset).</summary>
        private const int DatapointLen = 32;

        public SensorDatapoint(double value, DateTimeOffset timestamp, TimeSpan duration)
        {
            Value = value;
            Timestamp = timestamp;
            Duration = duration;
        }

        public TimeSpan Duration { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public double Value { get; set; }

        public static List<SensorDatapoint> Deserialise(byte[] bytes)
        {
            if (bytes.Length%DatapointLen != 0)
                throw new ArgumentException($"Invalid data, length not a multiple of {DatapointLen}.");

            var datapoints = new List<SensorDatapoint>(bytes.Length/DatapointLen);
            for (var i = 0; i < bytes.Length; i += DatapointLen)
            {
                var value = BitConverter.ToDouble(bytes, i);
                var durationInTicks = BitConverter.ToInt64(bytes, i + 8);
                var dateTimeInTicks = BitConverter.ToInt64(bytes, i + 16);
                var offsetInTicks = BitConverter.ToInt64(bytes, i + 24);
                datapoints.Add(new SensorDatapoint(value,
                    new DateTimeOffset(dateTimeInTicks, new TimeSpan(offsetInTicks)),
                    new TimeSpan(durationInTicks)));
            }
            return datapoints;
        }

        public static byte[] Serialise(List<SensorDatapoint> datapoints)
        {
            // Calculating the size isn't necessary but it works as a sanity check.
            var size = DatapointLen*datapoints.Count;

            var bytes = new List<byte>(size);
            foreach (var dp in datapoints)
            {
                var binValue = BitConverter.GetBytes(dp.Value);
                var binDuration = BitConverter.GetBytes(dp.Duration.Ticks);
                var binTicks = BitConverter.GetBytes(dp.Timestamp.Ticks);
                // Must store the original offset the data was stored in.
                // The local offset by change with summer time or different countries, the original should be kept.
                var binOffset = BitConverter.GetBytes(dp.Timestamp.Offset.Ticks);
                bytes.AddRange(binValue);
                bytes.AddRange(binDuration);
                bytes.AddRange(binTicks);
                bytes.AddRange(binOffset);
            }

            Debug.Assert(bytes.Count == size);

            return bytes.ToArray();
        }

        /// <summary>Datapoints in 'b' that match a timestamp in 'a' will be deleted before merge.</summary>
        /// <param name="a">Serialised datapoints, all of these are kept.</param>
        /// <param name="b">Serialise datapoints, items with timestamps that exist in 'a' are deleted.</param>
        /// <returns>The merged data serialised.</returns>
        public static byte[] Merge(byte[] a, byte[] b)
        {
            var aData = Deserialise(a);
            var bData = Deserialise(b);
            // Match items via the timestamps.
            var aTimestamps = aData.Select(d => d.Timestamp);
            var bDupes = bData.Where(bDatapoint => aTimestamps.Contains(bDatapoint.Timestamp));
            bData.RemoveAll(bDupes.Contains);
            // a and b data now contain items with different timestamps so join, serialise and return.
            aData.AddRange(bData);
            return Serialise(aData);
        }

        /// <summary>Extracts the data from start to end inclusive.</summary>
        /// <param name="data">Data to slice.</param>
        /// <param name="start">The earliest datetime to include.</param>
        /// <param name="end">The latest datetime to include.</param>
        /// <returns>A fresh data object.</returns>
        public static byte[] Slice(byte[] data, DateTimeOffset start, DateTimeOffset end)
        {
            var datapoints = Deserialise(data);
            var valid = datapoints.Where(d => (d.Timestamp >= start) && (d.Timestamp <= end)).ToList();
            return Serialise(valid);
        }
    }
}