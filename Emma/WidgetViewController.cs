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

        private nfloat height = 250; // 3 x 60f(cells),
        private nfloat marginTop = 10;

        public override void LoadView ()
        {
            base.LoadView ();

            var v = new UIView {
                BackgroundColor = UIColor.Clear,
                Frame = new CoreGraphics.CGRect ( 0,0, UIScreen.MainScreen.Bounds.Width, height),
            };

            v.Add (tableView = new UITableView {
                TranslatesAutoresizingMaskIntoConstraints = false,
                BackgroundColor = UIColor.Clear,
                TableFooterView = new UIView(),
                ScrollEnabled = false,
                RowHeight = 60f,
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

            // First update of data from UserDefaults

            // remove
            var projects = new List<ProjectData> ();

            var p = new ProjectData {
                ProjectName = "Empty project",
                ClientName = "",
                Color = ConvertUIColortoHex ( UIColor.White),
                IsEmpty = true,
                TimeValue = "00:00:00",
                IsRunning = false
            };

            projects.Add ( p);

            for (int i = 0; i < 3; i++) {
                p = new ProjectData {
                    ProjectName = "Project Name " + i,
                    ClientName = "Client Name " + i,
                    Color = ConvertUIColortoHex ( UIColor.White),
                    IsEmpty = false,
                    TimeValue = "00:00:0" + i,
                };
                projects.Add (p);
            }

            tableView.RegisterClassForCellReuse (typeof (WidgetProjectCell), WidgetProjectCell.WidgetProjectCellId);
            tableView.Source = new ProjectDataSource ( projects);
            tableView.Delegate = new ProjectTableViewDelegate ();

            Timer.Elapsed += OnTimedEvent;
            Timer.Start ();

            UpdateContent ();
        }

        private string ConvertUIColortoHex ( UIColor color)
        {
            nfloat fred;
            nfloat fblue;
            nfloat fgreen;
            nfloat alpha;
            color.GetRGBA ( out fred, out fblue, out fgreen, out alpha);

            var r = (nint)Math.Round (fred * 255);
            var b = (nint)Math.Round (fblue * 255);
            var g = (nint)Math.Round (fgreen * 255);
            return "#" + r.ToString ("X2") + g.ToString ("X2") + b.ToString ("X2");
        }

        public override void ViewDidUnload ()
        {
            Timer.Elapsed -= OnTimedEvent;
            Timer.Stop ();
            base.ViewDidUnload ();
        }

        private void UpdateContent()
        {
            // Periodicall update content from UserDefaults
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
            defaultMarginInsets.Left = 0f;
            defaultMarginInsets.Bottom = 0f;
            defaultMarginInsets.Top = marginTop;
            return defaultMarginInsets;
        }

        internal class ProjectDataSource : UITableViewSource
        {
            readonly List<ProjectData> items;

            public ProjectDataSource ( List<ProjectData> items)
            {
                this.items = items;
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                var cell = (WidgetProjectCell) tableView.DequeueReusableCell (WidgetProjectCell.WidgetProjectCellId, indexPath);
                cell.TranslatesAutoresizingMaskIntoConstraints = false;
                cell.Data = items[ indexPath.Row];
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
                    var separator = cell.SeparatorInset;
                    separator.Left = 50;
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
