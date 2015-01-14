using System;
using UIKit;
using Foundation;
using NotificationCenter;
using Cirrious.FluentLayouts.Touch;
using System.Collections.Generic;
using Toggl.Emma.Views;
using System.Timers;

namespace Toggl.Emma
{
    [Register ("WidgetViewController")]
    public class WidgetViewController : UIViewController, INCWidgetProviding
    {
        public static string IsStartedKey = "is_started_key";
        public static string MillisecondsKey = "milliseconds_key";

        private TopView topView;
        private UITableView tableView;
        private Timer timer;
        private NSUserDefaults nsUserDefaults;

        public NSUserDefaults UserDefaults
        {
            get {
                if ( nsUserDefaults == null) {
                    nsUserDefaults = new NSUserDefaults ("group.com.toggl.dummycontainer", NSUserDefaultsType.SuiteName);
                }
                return nsUserDefaults;
            }
        }

        public Timer Timer
        {
            get {
                if (timer == null)
                    timer = new Timer {
                    Interval = 1000,
                };
                return timer;
            }
        }

        public override void LoadView ()
        {
            base.LoadView ();

            var v = new UIView {
                BackgroundColor = UIColor.Clear
            };

            v.Add (topView = new TopView {
                TranslatesAutoresizingMaskIntoConstraints = false,
            });

            v.Add (tableView = new UITableView {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Clear,
                TableFooterView = new UIView(),
                ScrollEnabled = false,
                RowHeight = 60f,
            });

            v.AddConstraints (
                topView.AtTopOf (v),
                topView.WithSameWidth (v),
                topView.Height().EqualTo ( 60f),

                tableView.WithSameWidth ( v),
                tableView.Below ( topView),
                tableView.Height().EqualTo ( 180f).SetPriority ( UILayoutPriority.DefaultLow), // 3 x 60f(cells),
                tableView.AtBottomOf ( v),

                null
            );

            View = v;
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            topView.StartBtnPressed += (sender, e) => nsUserDefaults.SetBool (topView.IsRunning, IsStartedKey);

            // remove
            var projects = new List<string> ();
            var random = new Random ();
            for (int i = 0; i < 3; i++) {
                projects.Add ("Project - " + random.Next ());
            }

            tableView.RegisterClassForCellReuse (typeof (WidgetProjectCell), WidgetProjectCell.WidgetProjectCellId);
            tableView.Source = new ProjectDataSource ( projects);
            tableView.Delegate = new ProjectTableViewDelegate ();

            Timer.Elapsed += OnTimedEvent;
            Timer.Start ();

            UpdateContent ();
        }

        public override void ViewDidUnload ()
        {
            Timer.Elapsed -= OnTimedEvent;
            Timer.Stop ();
            base.ViewDidUnload ();
        }

        private void UpdateContent()
        {
            topView.IsRunning = UserDefaults.BoolForKey (IsStartedKey);
            string time = UserDefaults.StringForKey (MillisecondsKey);
            topView.TimeValue = string.IsNullOrEmpty (time) ? topView.TimeValue : time;
        }

        private void OnTimedEvent (object source, ElapsedEventArgs e)
        {
            InvokeOnMainThread (UpdateContent);
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
            defaultMarginInsets.Bottom = 0f;
            return defaultMarginInsets;
        }

        internal class ProjectDataSource : UITableViewSource
        {
            readonly List<string> items;

            public ProjectDataSource ( List<string> items)
            {
                this.items = items;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var cell = (WidgetProjectCell) tableView.DequeueReusableCell (WidgetProjectCell.WidgetProjectCellId, indexPath);
                cell.TranslatesAutoresizingMaskIntoConstraints = false;
                cell.ProjectName = items[ indexPath.Row];
                cell.IndentationLevel = 0;
                return cell;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return (nint)items.Count;
            }
        }

        internal class ProjectTableViewDelegate : UITableViewDelegate
        {
            public override void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
            {
                cell.BackgroundColor = UIColor.Clear;

                if (cell.RespondsToSelector (new ObjCRuntime.Selector ("setSeparatorInset:"))) {
                    cell.SeparatorInset = UIEdgeInsets.Zero;
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
