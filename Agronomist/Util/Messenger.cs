namespace Agronomist.Util
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    ///     A middleman class for passing information between parts of the program.
    /// </summary>
    public class Messenger
    {
        private readonly List<WeakReference<Action<string>>> _onNewDeviceList =
            new List<WeakReference<Action<string>>>();

        private readonly List<WeakReference<Action<IEnumerable<SensorDataPoint>>>> _onNewLiveDataPoint =
            new List<WeakReference<Action<IEnumerable<SensorDataPoint>>>>();

        public void AddOnNewDevice(Action<IEnumerable<SensorDataPoint>> action)
        {
            _onNewLiveDataPoint.Add(new WeakReference<Action<IEnumerable<SensorDataPoint>>>(action));
        }

        public void AddOnNewDevice(Action<string> action)
        {
            _onNewDeviceList.Add(new WeakReference<Action<string>>(action));
        }

        public async Task OnNewLiveDataPoint(IEnumerable<SensorDataPoint> data)
        {
            await PruneRefsAndFireActions(data, _onNewLiveDataPoint);
        }

        public async Task OnNewDevice(string deviceId)
        {
            await PruneRefsAndFireActions(deviceId, _onNewDeviceList);
        }

        private async Task PruneRefsAndFireActions<T>(T param, List<WeakReference<Action<T>>> weakList)
        {
            //The first half of this code has externally mutable lists, so no awaits.
            var strongRefs = new List<Action<T>>();
            var deleteMe = new List<WeakReference<Action<T>>>();
            foreach (var weakReference in weakList)
            {
                Action<T> target;
                if (weakReference.TryGetTarget(out target))
                {
                    strongRefs.Add(target);
                }
                else
                {
                    deleteMe.Add(weakReference);
                }
            }

            foreach (var weakReference in deleteMe)
            {
                weakList.Remove(weakReference);
            }

            // Looping through external lists complete, can await now.
            foreach (var strongRef in strongRefs)
            {
                await new Task(() => strongRef(param));
            }
        }

        public struct SensorDataPoint
        {
            public SensorDataPoint(Guid id, double value, DateTimeOffset timestamp, TimeSpan duration)
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

        #region SingletonInit

        private static readonly Messenger SingletonInstance = new Messenger();

        private Messenger()
        {
        }

        public Messenger Instance => SingletonInstance;

        #endregion
    }
}