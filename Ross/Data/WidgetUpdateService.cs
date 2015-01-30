using System;
using System.Collections.Generic;
using Foundation;
using Newtonsoft.Json;
using Toggl.Phoebe;

namespace Toggl.Ross.Data
{
    public class WidgetUpdateService : IWidgetUpdateService
    {
        public static string MillisecondsKey = "milliseconds_key";
        public static string TimeEntriesKey = "time_entries_key";
        public static string StartedEntryKey = "started_entry_key";

        private NSUserDefaults nsUserDefaults;

        public NSUserDefaults UserDefaults
        {
            get {
                if ( nsUserDefaults == null) {
                    nsUserDefaults = new NSUserDefaults ("group.com.toggl.timer", NSUserDefaultsType.SuiteName);
                }
                return nsUserDefaults;
            }
        }

        #region IWidgetUpdateService implementation

        public void SetLastEntries ( List<WidgetSyncManager.WidgetEntryData> lastEntries)
        {
            if ( lastEntries != null) {
                var json = JsonConvert.SerializeObject ( lastEntries);
                UserDefaults.SetString ( json, TimeEntriesKey);
            }
        }

        public void SetRunningEntryDuration (string duration)
        {
            UserDefaults.SetString ( duration, MillisecondsKey);
        }

        public long GetEntryIdStarted ()
        {
            return long.Parse ( UserDefaults.StringForKey ( StartedEntryKey));
        }

        public long GetEntryIdStopped ()
        {
            throw new NotImplementedException ();
        }

        #endregion
    }
}

