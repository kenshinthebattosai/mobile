using System.Collections.Generic;

namespace Toggl.Phoebe
{
    public interface IWidgetUpdateService
    {
        void SetLastEntries ( List<WidgetSyncManager.WidgetEntryData> lastEntries);

        void SetRunningEntryDuration ( string duration);

        long GetEntryIdStarted();

        long GetEntryIdStopped();
    }
}