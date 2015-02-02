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
        public static string StartedEntryKey = "started_entry_key";
        public static string ViewedEntryKey = "viewed_entry_key";
        public static string IsUserLoggedKey = "is_logged_key";

        private UITableView tableView;
        private UITextView textView;
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

            v.Add (textView = new UITextView {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Font = UIFont.FromName ( "Helvetica", 14f),
                Text = "NoLoggedUser".Tr(),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.Clear,
                Hidden = true,
            });

            v.AddConstraints (

                tableView.AtTopOf (v),
                tableView.WithSameWidth ( v),
                tableView.Height().EqualTo ( height - marginTop).SetPriority ( UILayoutPriority.DefaultLow),
                tableView.AtBottomOf ( v),

                textView.WithSameCenterX ( v),
                textView.WithSameCenterY ( v),
                textView.WithSameWidth ( v),
                textView.Height().EqualTo ( cellHeight),

                null
            );

            View = v;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            // Check if user is logged
            var isLogged = UserDefaults.BoolForKey ( IsUserLoggedKey);
            if ( !isLogged) {
                tableView.Hidden = true;
                textView.Hidden = false;
                return;
            }

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
                entries.Add ( new WidgetEntryData { IsEmpty = true });
            }

            tableView.RegisterClassForCellReuse (typeof (WidgetCell), WidgetCell.WidgetProjectCellId);

            var source = new TableDataSource ( entries);
            var tvdelegate = new TableViewDelegate ();
            tableView.Source = source;
            tableView.Delegate = tvdelegate;

            tvdelegate.OnPressCell += (sender, e) => {
                var id = string.IsNullOrEmpty ( tvdelegate.SelectedCellId) ? new Guid().ToString() : tvdelegate.SelectedCellId;
                UserDefaults.SetString ( id, ViewedEntryKey);
                UIApplication.SharedApplication.OpenUrl (new NSUrl ("com.toggl.timer://"));
            };

            source.OnPressPlayOnCell += (sender, e) => {
                var id = string.IsNullOrEmpty ( source.SelectedCellId) ? new Guid().ToString() : source.SelectedCellId;
                UserDefaults.SetString ( id, StartedEntryKey);
                UIApplication.SharedApplication.OpenUrl (new NSUrl ("com.toggl.timer://"));
            };

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

            public event EventHandler OnPressPlayOnCell;

            public string SelectedCellId { get; set; }

            public TableDataSource ( List<WidgetEntryData> items)
            {
                this.items = items;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var cell = (WidgetCell) tableView.DequeueReusableCell (WidgetCell.WidgetProjectCellId, indexPath);
                cell.TranslatesAutoresizingMaskIntoConstraints = false;
                cell.Data = items[ indexPath.Row];

                cell.StartBtnPressed += (sender, e) => {
                    if ( OnPressPlayOnCell != null) {
                        SelectedCellId = items[ indexPath.Row].Id;
                        OnPressPlayOnCell.Invoke ( this, new EventArgs());
                    }
                };
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

            public event EventHandler OnPressCell;

            public string SelectedCellId { get; set; }

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

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                if ( OnPressCell != null) {
                    var cell = (WidgetCell)tableView.CellAt ( indexPath);
                    SelectedCellId = cell.Data.Id;
                    OnPressCell.Invoke ( this, new EventArgs());
                }
            }

        }
    }
}
