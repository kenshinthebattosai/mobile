using System.Collections.Generic;
using System;

namespace Toggl.Phoebe
{
    public interface IWidgetUpdateService
    {
        void SetLastEntries ( List<WidgetSyncManager.WidgetEntryData> lastEntries);

        void SetRunningEntryDuration ( string duration);

        void SetUserLogged ( bool isLogged);

        Guid GetEntryIdStarted();

        Guid GetEntryIdViewed();
    }
}