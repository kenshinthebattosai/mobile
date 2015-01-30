using System;
using System.Collections.Generic;
using System.Timers;
using Cirrious.FluentLayouts.Touch;
using CoreGraphics;
using Foundation;
using Newtonsoft.Json;
using NotificationCenter;
using Toggl.Emma.Views;
using UIKit;

namespace Toggl.Emma
{
    [Register ("WidgetViewController")]
    public class WidgetViewController : UIViewController, INCWidgetProviding
    {
        public static string MillisecondsKey = "milliseconds_key";
        public static string TimeEntriesKey = "time_entries_key";

        private UITableView tableView;
        private Timer timer;
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

        public Timer UpdateTimer
        {
            get {
                if (timer == null)
                    timer = new Timer {
                    Interval = 1000,
                };
                return timer;
            }
        }

        private nfloat cellHeight = 60;
        private nfloat height = 250; // 4 x 60f(cells),
        private nfloat marginTop = 10;

        public override void LoadView ()
        {
            base.LoadView ();

            var v = new UIView {
                BackgroundColor = UIColor.Clear,
                Frame = new CGRect ( 0,0, UIScreen.MainScreen.Bounds.Width, height),
            };

            v.Add (tableView = new UITableView {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Clear,
                TableFooterView = new UIView(),
                ScrollEnabled = false,
                RowHeight = cellHeight,
            });

            v.AddConstraints (

                tableView.AtTopOf (v),
                tableView.WithSameWidth ( v),
                tableView.Height().EqualTo ( height - marginTop).SetPriority ( UILayoutPriority.DefaultLow),
                tableView.AtBottomOf ( v),

                null
            );

            View = v;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            // Get saved entries
            var entries = new List<WidgetEntryData>();

            var timeEntryJson = UserDefaults.StringForKey ( TimeEntriesKey);
            if ( timeEntryJson != string.Empty) {
                entries = JsonConvert.DeserializeObject<List<WidgetEntryData>> ( timeEntryJson);
            }

            bool isRunning = false;

            if ( entries.Count > 0) {

                // Check running state
                foreach (var item in entries) {
                    isRunning = isRunning || item.IsRunning;
                }

                if ( !isRunning) {
                    // Add empty cell at top
                    var emptyEntry = new WidgetEntryData { IsEmpty = true };
                    entries.Insert ( 0, emptyEntry);
                    entries.RemoveAt ( entries.Count - 1);
                }

            } else {
                entries.Add ( new WidgetEntryData { IsRunning = false, IsEmpty = true });
            }

            tableView.RegisterClassForCellReuse (typeof (WidgetCell), WidgetCell.WidgetProjectCellId);
            tableView.Source = new TableDataSource ( entries);
            tableView.Delegate = new TableViewDelegate ();

            if ( isRunning) {
                // Start to check time
                UpdateTimer.Elapsed += ( sender, e) => InvokeOnMainThread (UpdateContent);
                UpdateTimer.Start();
            }

            UpdateContent ();
        }

        public override void ViewDidUnload ()
        {
            UpdateTimer.Stop ();
            base.ViewDidUnload ();
        }

        private void UpdateContent()
        {
            // Periodically update content from UserDefaults
            var timeValue = UserDefaults.StringForKey ( MillisecondsKey);

            if ( !string.IsNullOrEmpty ( timeValue)) {
                var cell = ( WidgetCell)tableView.CellAt ( NSIndexPath.FromRowSection ( 0, 0));
                if ( cell != null) {
                    cell.TimeValue = UserDefaults.StringForKey ( MillisecondsKey);
                }
            }
        }

        [Export ("widgetPerformUpdateWithCompletionHandler:")]
        public void WidgetPerformUpdate (Action<NCUpdateResult> completionHandler)
        {
            UpdateContent ();
            completionHandler (NCUpdateResult.NewData);
        }

        [Export ("widgetMarginInsetsForProposedMarginInsets:")]
        public UIEdgeInsets GetWidgetMarginInsets (UIEdgeInsets defaultMarginInsets)
        {
            defaultMarginInsets.Left = 0f;
            defaultMarginInsets.Bottom = 0f;
            defaultMarginInsets.Top = marginTop;
            return defaultMarginInsets;
        }

        internal class TableDataSource : UITableViewSource
        {
            readonly List<WidgetEntryData> items;

            public TableDataSource ( List<WidgetEntryData> items)
            {
                this.items = items;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var cell = (WidgetCell) tableView.DequeueReusableCell (WidgetCell.WidgetProjectCellId, indexPath);
                cell.TranslatesAutoresizingMaskIntoConstraints = false;
                cell.Data = items[ indexPath.Row];
                return cell;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return (nint)items.Count;
            }
        }

        internal class TableViewDelegate : UITableViewDelegate
        {
            int maxCellNum = 3;
            nfloat borderMargin = 50;

            public override void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                cell.BackgroundColor = UIColor.Clear;

                if (cell.RespondsToSelector (new ObjCRuntime.Selector ("setSeparatorInset:"))) {
                    var separator = cell.SeparatorInset;
                    separator.Left = (indexPath.Row < maxCellNum) ? borderMargin : tableView.Bounds.Width;
                    cell.SeparatorInset = separator;
                }

                if (cell.RespondsToSelector (new ObjCRuntime.Selector ("setPreservesSuperviewLayoutMargins:"))) {
                    cell.PreservesSuperviewLayoutMargins = false;
                }

                if (cell.RespondsToSelector (new ObjCRuntime.Selector ("setLayoutMargins:"))) {
                    cell.LayoutMargins = UIEdgeInsets.Zero;
                }
            }
        }
    }
}
