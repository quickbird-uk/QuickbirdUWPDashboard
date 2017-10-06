using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation.Diagnostics;
using Windows.Storage;

namespace Quickbird.Util
{
    public static class LoggingService
    {

        static LoggingSession _Session; 
        private static LoggingChannel _LcDebug;
        private static LoggingChannel _LcTrace; 

        private static UInt64 tracingIndex = 0;
        private static TraceEvent[] TracingData = new TraceEvent[200];

        /// <summary>
        /// LoggingSessionScenario moves generated logs files into the
        /// this folder under the LocalState folder.
        /// </summary>
        public const string LOG_FILE_FOLDER_NAME = "LogFiles";

        static LoggingService()
        {   
            _LcDebug = new LoggingChannel("QuickbirdUWP_Log", 
                new LoggingChannelOptions(
                    new Guid("d3020f82-b5bd-4ead-b739-a2e043d075f3")
                    ));
            _LcTrace = new LoggingChannel("QuickbirdUWP_trace",
                new LoggingChannelOptions(
                    new Guid("e4fd6bbb-74de-456a-981e-85314c56c875")
                    ));

            _Session = new LoggingSession("AppWideSession");
            _Session.AddLoggingChannel(_LcDebug);
            _Session.AddLoggingChannel(_LcTrace);
        }

        /// <summary>
        /// Errors that should never happen, butthey happen none the less.
        /// </summary>
        /// <param name="description"></param>
        /// <param name="level"></param>
        public static void LogInfo(string description, LoggingLevel level, [CallerMemberName]string caller = "Unknown") {
            string title = Enum.GetName(typeof(LoggingLevel), level);

            if (level >= LoggingLevel.Error)
            { 
                ToastService.NotifyUserOfError(description);
            }
            else if(level >= LoggingLevel.Information)
            {
                ToastService.Debug(caller, description); 
            }

            LoggingFields fields = new LoggingFields();
            fields.AddString("description", description);
            fields.AddString("Callermember", caller);
            fields.AddDateTime("Timestamp", DateTime.Now);

            _LcDebug.LogEvent("Event", fields, level); 
        }

        //Save trace data, will only store last 200 if the app crashes.
        public static void Trace(string what, [CallerMemberName]string caller = "Unknown")
        {
            TraceEvent.AddTrace(what, caller, DateTimeOffset.Now); 
        }

        /// <summary>
        /// Meant to be called when the app is crashing, will save the log. 
        /// </summary>
        public static void SaveLog()
        {
            var traces = TraceEvent.TraceBuffer;
            foreach (var trace in traces)
            {
                LoggingFields fields = new LoggingFields();
                fields.AddString("description", trace.Description);
                fields.AddString("Callermember", trace.CallerMember);
                fields.AddDateTime("Timestamp", trace.Timestamp.LocalDateTime);
                _LcTrace.LogEvent("TraceEvent", fields);
            }

            var saving = Task.Run(SaveLogInMemoryToFileAsync);
            saving.Wait(); 
        }

        private static async Task<string> SaveLogInMemoryToFileAsync()
        {
            StorageFolder sampleAppDefinedLogFolder =
                await ApplicationData.Current.LocalFolder.CreateFolderAsync(LOG_FILE_FOLDER_NAME,
                                                                            CreationCollisionOption.OpenIfExists);
            string newLogFileName = "Log-" + GetTimeStamp() + ".etl";
            StorageFile newLogFile = await _Session.SaveToFileAsync(sampleAppDefinedLogFolder, newLogFileName);
            if (newLogFile != null)
            {
                return newLogFile.Path;
            }
            else
            {
                return null;
            }
        }

        private static string GetTimeStamp()
        {
            DateTime now = DateTime.Now;
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                 "{0:D2}{1:D2}{2:D2}-{3:D2}{4:D2}{5:D2}{6:D3}",
                                 now.Year - 2000,
                                 now.Month,
                                 now.Day,
                                 now.Hour,
                                 now.Minute,
                                 now.Second,
                                 now.Millisecond);
        }


        /// <summary>
        /// Trace is for unimportant events, which will only be recorded in the event 
        /// of a crash or something equally dramatic happening. Otherwise they are usually discarded.
        /// They jsut register flow of the program. 
        /// </summary>
        private struct TraceEvent
        {
            private const int _TraceBufferSize = 200; 
            private static Queue<TraceEvent> _TraceBuffer = new Queue<TraceEvent>(_TraceBufferSize); //Circular buffer
            private static UInt64 _TracesLogged = 0;

            public static void AddTrace(string description, string callerMemberName, DateTimeOffset timestamp)
            {
                while(_TraceBuffer.Count >= _TraceBufferSize)
                    { _TraceBuffer.Dequeue(); }

                TraceEvent trace = new TraceEvent
                {
                    Timestamp = timestamp,
                    CallerMember = callerMemberName,
                    Description = description
                };
                _TraceBuffer.Enqueue(trace); 
                _TracesLogged++;
            }

            public static TraceEvent[] TraceBuffer => _TraceBuffer.ToArray();

            public static UInt64 TracesLogged => _TracesLogged;

            public string Description;
            public DateTimeOffset Timestamp;
            public string CallerMember;            
        }
    }
}
