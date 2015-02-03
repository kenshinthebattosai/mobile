using System;
using System.Collections.Generic;
using Toggl.Phoebe.Data.Models;

namespace Toggl.Phoebe
{
    public interface IWidgetUpdateService
    {
        void SetLastEntries ( List<WidgetSyncManager.WidgetEntryData> lastEntries);

        void SetRunningEntryDuration ( string duration);

        void SetUserLogged ( bool isLogged);

        void ShowNewTimeEntryScreen ( TimeEntryModel currentTimeEntry);

        Guid GetEntryIdStarted();
    }
}