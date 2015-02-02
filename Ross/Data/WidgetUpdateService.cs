using System;
using System.Collections.Generic;
using Foundation;
using Newtonsoft.Json;
using Toggl.Phoebe;
using Toggl.Phoebe.Net;

namespace Toggl.Ross.Data
{
    public class WidgetUpdateService : IWidgetUpdateService
    {
        public static string MillisecondsKey = "milliseconds_key";
        public static string TimeEntriesKey = "time_entries_key";
        public static string StartedEntryKey = "started_entry_key";
        public static string ViewedEntryKey = "viewed_entry_key";
        public static string IsUserLoggedKey = "is_logged_key";

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

        public WidgetUpdateService ()
        {
            // Update auth state from platform service.
            //
            // Phoebe services are initializated first,
            // if we try to update auth state from Phoebe
            // WidgetUpdateService still doesn't exists.

            var authManager = XPlatUtils.ServiceContainer.Resolve<AuthManager>();
            SetUserLogged ( authManager.IsAuthenticated);
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

        public void SetUserLogged (bool isLogged)
        {
            UserDefaults.SetBool ( isLogged, IsUserLoggedKey);
        }

        public Guid GetEntryIdStarted ()
        {
            Guid entryId;
            Guid.TryParse ( UserDefaults.StringForKey ( StartedEntryKey), out entryId);
            return entryId;
        }

        public Guid GetEntryIdViewed ()
        {
            Guid entryId;
            Guid.TryParse ( UserDefaults.StringForKey ( ViewedEntryKey), out entryId);
            return entryId;
        }

        #endregion
    }
}

