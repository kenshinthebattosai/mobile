using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using Toggl.Phoebe.Data;
using Toggl.Phoebe.Data.DataObjects;
using Toggl.Phoebe.Data.Models;
using Toggl.Phoebe.Logging;
using Toggl.Phoebe.Net;
using XPlatUtils;

namespace Toggl.Phoebe
{
    public class WidgetSyncManager
    {
        private const string Tag = "WidgetSyncManager";
        private const string DefaultDurationText = " 00:00:00 ";

        private ActiveTimeEntryManager timeEntryManager;
        private TimeEntryModel currentTimeEntry;
        private AuthManager authManager;
        private IWidgetUpdateService widgetUpdateService;
        private Subscription<SyncStartedMessage> subscriptionSyncStarted;
        private Subscription<SyncFinishedMessage> subscriptionSyncFinished;

        private bool isLoading;
        private int rebindCounter;

        public WidgetSyncManager ()
        {
            authManager = ServiceContainer.Resolve<AuthManager>();
            authManager.PropertyChanged += OnAuthPropertyChanged;
            //widgetUpdateService.SetUserLogged( authManager.IsAuthenticated);

            widgetUpdateService = ServiceContainer.Resolve<IWidgetUpdateService>();
            timeEntryManager = ServiceContainer.Resolve<ActiveTimeEntryManager> ();
            timeEntryManager.PropertyChanged += OnTimeEntryManagerPropertyChanged;

            var bus = ServiceContainer.Resolve<MessageBus> ();
            subscriptionSyncStarted = bus.Subscribe<SyncStartedMessage> (OnSync);
            subscriptionSyncFinished = bus.Subscribe<SyncFinishedMessage> (OnSync);
        }

        public void Dispose ()
        {
            var bus = ServiceContainer.Resolve<MessageBus> ();

            if (subscriptionSyncStarted != null) {
                bus.Unsubscribe (subscriptionSyncStarted);
                subscriptionSyncStarted = null;
            }

            if (subscriptionSyncFinished != null) {
                bus.Unsubscribe (subscriptionSyncFinished);
                subscriptionSyncFinished = null;
            }
        }

        private async void OnSync (Message msg)
        {
            if ( isLoading) {
                return;
            }

            isLoading = true;

            try {
                var queryStartDate = Time.UtcNow - TimeSpan.FromDays (9);

                // Query local data:
                var store = ServiceContainer.Resolve<IDataStore> ();
                var userId = ServiceContainer.Resolve<AuthManager> ().GetUserId ();

                var baseQuery = store.Table<TimeEntryData> ()
                                .OrderBy (r => r.StartTime, false)
                                .Where (r => r.DeletedAt == null
                                        && r.UserId == userId
                                        && r.State != TimeEntryState.New
                                        && r.StartTime >= queryStartDate)
                                .Take ( 5);

                var entries = await baseQuery.QueryAsync ().ConfigureAwait (false);
                var widgetEntries = new List<WidgetEntryData>();

                foreach (var entry in entries) {

                    ProjectData project;

                    if ( entry.ProjectId != null) {
                        var q = store.Table<ProjectData>().Where ( p => p.Id == entry.ProjectId.Value);
                        var l = await q.QueryAsync ().ConfigureAwait (false);
                        project = l.FirstOrDefault();
                    } else {
                        project = new ProjectData {
                            Name = string.Empty,
                            Color = ProjectModel.HexColors.Length - 1,
                        };
                    }

                    widgetEntries.Add ( new WidgetEntryData {
                        Id = entry.Id.ToString(),
                        ProjectName = project.Name,
                        Description = entry.Description,
                        Color = ProjectModel.HexColors [ project.Color % ProjectModel.HexColors.Length],
                        IsRunning = entry.State == TimeEntryState.Running,
                        TimeValue = (entry.StopTime - entry.StartTime).ToString(),
                    });

                }

                widgetUpdateService.SetLastEntries ( widgetEntries);

            } catch (Exception exc) {
                var log = ServiceContainer.Resolve<ILogger> ();
                log.Error (Tag, exc, "Failed to fetch time entries");
            } finally {
                isLoading = false;
            }
        }

        private void OnAuthPropertyChanged (object sender, PropertyChangedEventArgs args)
        {
            if ( args.PropertyName == AuthManager.PropertyIsAuthenticated) {
                widgetUpdateService.SetUserLogged ( authManager.IsAuthenticated);
            }
        }

        private void OnTimeEntryManagerPropertyChanged (object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == ActiveTimeEntryManager.PropertyRunning) {
                ResetModelToRunning ();
                Rebind ();
            }
        }

        private void ResetModelToRunning ()
        {
            if (timeEntryManager == null) {
                return;
            }

            if (currentTimeEntry == null) {
                currentTimeEntry = (TimeEntryModel)timeEntryManager.Running;
            } else if (timeEntryManager.Running != null) {
                currentTimeEntry.Data = timeEntryManager.Running;
            } else {
                currentTimeEntry = null;
            }
        }

        private void Rebind ()
        {
            rebindCounter++;

            if (currentTimeEntry == null) {
                widgetUpdateService.SetRunningEntryDuration ( DefaultDurationText);
            } else {
                var duration = currentTimeEntry.GetDuration ();
                widgetUpdateService.SetRunningEntryDuration ( duration.ToString (@"hh\:mm\:ss"));

                var counter = rebindCounter;
                var timer = new Timer ( 1000 - duration.Milliseconds);
                timer.Elapsed += (sender, e) => {
                    if (counter == rebindCounter) {
                        Rebind ();
                    }
                };
                timer.Start();
            }
        }

        public class WidgetEntryData
        {
            public string Id { get; set; }

            public string ProjectName { get; set; }

            public string Description { get; set; }

            public string TimeValue { get; set; }

            public string Color { get; set; }

            public bool IsRunning { get; set; }
        }
    }
}

